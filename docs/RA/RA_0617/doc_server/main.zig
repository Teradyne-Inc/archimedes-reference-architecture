const std = @import("std");

const ArgParseError = error{
    bad_args,
};

const ParsedArgs = struct {
    doc_directory: []const u8,
    help_requested: bool,
};

const usage = "Usage: serve_docs <PATH> [-h]";

fn parseArgs(args: [][:0]u8) !ParsedArgs {
    if (args.len > 3) {
        return ArgParseError.bad_args;
    } else {
        var parsed_args: ParsedArgs = .{
            .doc_directory = args[1],
            .help_requested = false,
        };

        for (args) |arg| {
            if (std.mem.eql(u8, "-h", arg)) {
                parsed_args.help_requested = true;
            }
        }

        return parsed_args;
    }
}

fn printHelp() void {
    std.debug.print("{s}\n", .{usage});
}

pub fn main() !void {
    var debug_allocator: std.heap.DebugAllocator(.{}) = .init;
    const allocator: std.mem.Allocator = debug_allocator.allocator();
    defer _ = debug_allocator.deinit();

    const args: [][:0]u8 = try std.process.argsAlloc(allocator);
    defer std.process.argsFree(allocator, args);

    const parsed_args: ParsedArgs = try parseArgs(args);
    if (parsed_args.help_requested) {
        printHelp();
        return;
    }

    var stdout_buffer: [4096]u8 = undefined;
    var writer = std.fs.File.stdout().writer(&stdout_buffer);
    defer writer.interface.flush() catch @panic("flush error");

    var iter = try std.fs.path.componentIterator(parsed_args.doc_directory);
    while (iter.next()) |component| {
        _ = try writer.interface.write("Creating ");
        _ = try writer.interface.write(component.path);
        _ = try writer.interface.write("\n");
        std.fs.makeDirAbsolute(component.path) catch |e| {
            switch (e) {
                error.PathAlreadyExists => {
                    _ = try writer.interface.write(component.path);
                    _ = try writer.interface.write(" already exists!\n");
                },
                else => |other| return other,
            }
        };
    }

    _ = try writer.interface.write("\n");
    _ = try writer.interface.write("***************************************\n");
    _ = try writer.interface.write("Docs will be built in ");
    _ = try writer.interface.write(parsed_args.doc_directory);
    _ = try writer.interface.write("\n");
    _ = try writer.interface.write("Use an http server (like python -m http.server -d <PATH>)");
    _ = try writer.interface.write(" to view them.\n");
    _ = try writer.interface.write("***************************************\n");
}
