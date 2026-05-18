const std = @import("std");
const builtin = @import("builtin");

const Builder = struct {
    common_flags: []const []const u8,
    build_compiledb: bool,

    pub fn buildFlags(
        self: @This(),
        allocator: std.mem.Allocator,
        file: []const u8,
    ) std.mem.Allocator.Error![]const []const u8 {
        var buf: std.ArrayList([]const u8) = .empty;
        try buf.appendSlice(allocator, self.common_flags);

        if (self.build_compiledb) {
            try buf.append(allocator, "-MJ");
            try buf.append(allocator, tmp_file: {
                var tmp_name_buf: std.ArrayList(u8) = .empty;
                for (file) |letter| {
                    try tmp_name_buf.append(allocator, switch (letter) {
                        '/' => '.',
                        else => |other| other,
                    });
                }
                try tmp_name_buf.appendSlice(allocator, ".json.tmp");
                break :tmp_file tmp_name_buf.items;
            });
        }

        return buf.items;
    }
};

pub fn build(b: *std.Build) std.mem.Allocator.Error!void {
    const target = b.standardTargetOptions(.{});
    const optimize = b.standardOptimizeOption(.{});

    const depboost = b.dependency("boost", .{
        .target = target,
        .optimize = optimize,
        .json = true, // boost.json isn't header only and needs to be linked
    });
    const artifact_boost = depboost.artifact("boost");

    const create_compiledb: bool = b.option(
        bool,
        "compiledb",
        "Create compile_commands.json",
    ) orelse false;

    const norebin: bool = b.option(
        bool,
        "norebin",
        "Do not build and include Rebin (For binary size and performance).",
    ) orelse false;

    const nolistener: bool = b.option(
        bool,
        "nolistener",
        "Do not build and include unidirectional DAS (For binary size and performance).",
    ) orelse false;

    const nologs: bool = b.option(
        bool,
        "nologs",
        "Disable logging to file for performance",
    ) orelse false;

    const builder: Builder = .{
        .common_flags = &.{
            "-std=c++23",
            "-Wall",
            "-Wextra",
            "-Wpedantic",
            "-Wshadow",
            "-Wconversion",
            "-Wswitch-enum",
            "-Werror",
        },
        .build_compiledb = create_compiledb,
    };

    const files: []const []const u8 = comptime &.{
        "src/main.cpp",
    };

    const moddas = b.addModule("cppdas", .{
        .target = target,
        .optimize = optimize,
        .link_libcpp = true,
        .link_libc = false,
        .strip = (optimize != .Debug),
    });
    moddas.addIncludePath(b.path("src/include/"));

    inline for (files) |file| {
        moddas.addCSourceFile(.{
            .file = b.path(file),
            .language = .cpp,
            .flags = try builder.buildFlags(b.allocator, file),
        });
    }
    if (target.result.os.tag == .windows) {
        moddas.linkSystemLibrary("Ws2_32", .{});
        moddas.addCMacro("WIN32_LEAN_AND_MEAN", "");
    }
    if (nologs) {
        moddas.addCMacro("NOLOG", "");
    }
    if (nolistener) {
        moddas.addCMacro("NOLISTENER", "true");
    }
    if (norebin) {
        moddas.addCMacro("NOREBIN", "true");
    }

    for (artifact_boost.root_module.include_dirs.items) |dir| {
        moddas.addSystemIncludePath(dir.path);
    }
    moddas.linkLibrary(artifact_boost);

    const exedas = b.addExecutable(.{
        .name = "cppdas",
        .root_module = moddas,
    });
    exedas.lto = switch (optimize) {
        .Debug => .none,
        else => .full,
    };

    const install_das = b.addInstallArtifact(
        exedas,
        .{
            .dest_dir = .{
                .override = .{
                    .custom = out_path: {
                        var path: std.ArrayList(u8) = .empty;
                        const alloc = b.allocator;
                        try path.appendSlice(alloc, @tagName(target.result.cpu.arch));
                        try path.append(alloc, '-');
                        try path.appendSlice(alloc, @tagName(target.result.os.tag));
                        try path.append(alloc, '-');
                        try path.appendSlice(alloc, @tagName(target.result.abi));

                        // break :out_path path.items;
                        break :out_path b.pathResolve(&.{
                            path.items,
                            @tagName(optimize),
                        });
                    },
                },
            },
        },
    );
    b.getInstallStep().dependOn(&install_das.step);

    const run_step = b.step("run", "Run the app");
    const run_cmd = b.addRunArtifact(exedas);
    run_step.dependOn(&run_cmd.step);
    run_cmd.step.dependOn(b.getInstallStep());

    if (b.args) |args| {
        run_cmd.addArgs(args);
    }

    // Docs
    const doc_step = b.step("docs", "Build Doxygen documentation");

    const native_target = &builtin.target;
    const resolved_native_target = std.Target.Query.fromTarget(native_target);

    const mod_serve_docs = b.createModule(.{
        .target = b.resolveTargetQuery(resolved_native_target),
        .optimize = .Debug,
        .root_source_file = b.path("doc_server/main.zig"),
    });
    const exe_serve_docs = b.addExecutable(.{
        .name = "serve_docs",
        .root_module = mod_serve_docs,
    });

    const run_serve_docs = b.addRunArtifact(exe_serve_docs);
    const doc_path = b.pathResolve(&.{
        b.install_prefix,
        "docs",
        "html",
    });
    run_serve_docs.addArg(doc_path);

    const dep_doxygen: ?*std.Build.Dependency = native_calc: {
        break :native_calc switch (native_target.os.tag) {
            .windows => switch (native_target.cpu.arch) {
                .x86_64 => b.lazyDependency("doxygen-x86_64-windows", .{}),
                else => @panic("Invalid CPU architecture for Windows"),
            },
            .linux => switch (native_target.cpu.arch) {
                .x86_64 => @panic("TODO: Add doxy dep for linux"),
                else => @panic("Invalid CPU architecture for Linux"),
            },
            else => @panic("Unsupported native target"),
        };
    };

    if (dep_doxygen) |dep| {
        const run_doxygen = std.Build.Step.Run.create(b, "docs");
        run_doxygen.addFileArg(dep.path("doxygen"));

        run_doxygen.step.dependOn(&run_serve_docs.step);
        doc_step.dependOn(&run_doxygen.step);
    }

    // ******************** Optional CompileDB Creation ******************** //

    const modcompiledb = b.createModule(.{
        .optimize = optimize,
        .target = b.resolveTargetQuery(
            .fromTarget(&builtin.target), // must be native
        ),
        .root_source_file = b.path("cleandb/main.zig"),
    });
    const compiledb = b.addExecutable(.{
        .name = "compiledb",
        .root_module = modcompiledb,
    });
    const cleanup_command = b.addRunArtifact(compiledb);
    cleanup_command.addArgs(
        &.{
            std.process.getCwdAlloc(b.allocator) catch unreachable,
        },
    );

    cleanup_command.step.dependOn(&install_das.step);
    if (create_compiledb) {
        b.getInstallStep().dependOn(&cleanup_command.step);
    }
}
