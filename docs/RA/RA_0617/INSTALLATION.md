<p style="margin: 0; padding: 0;">
  <img src="media/RA617.png"
       alt="RA 617 Installation"
       style="width: 100%; height: auto; max-height: 240px; object-fit: cover;" />
</p>

# Zig Installation Guide

This guide explains how to install Zig 0.15.2 to build the boostdas project.

## Prerequisites

- Windows 10/11 (x64)
- Internet connection
- PowerShell 5.1 or later

## Required Version

This project requires **Zig version 0.15.2** exactly. Other versions may not work.

## Method 1: Manual Download (Recommended)

### Step 1: Download Zig

**Option A: Manual download**

1. Visit the official Zig downloads page:
   ```
   https://ziglang.org/download/
   ```

2. Download the Windows x64 version for 0.15.2:
   - **ZIP file (recommended)**: `zig-x86_64-windows-0.15.2.zip`
   - Direct URL: https://ziglang.org/download/0.15.2/zig-x86_64-windows-0.15.2.zip
   - Size: approximately 88 MB

**Option B: Automatic download via PowerShell**

Open PowerShell in the project folder and run:

```powershell
$url = "https://ziglang.org/download/0.15.2/zig-x86_64-windows-0.15.2.zip"
$output = "$PWD\zig.zip"
Invoke-WebRequest -Uri $url -OutFile $output -UseBasicParsing
Write-Host "✓ Downloaded: $([math]::Round((Get-Item $output).Length / 1MB, 2)) MB"
Expand-Archive -Path $output -DestinationPath $PWD -Force
Remove-Item $output
Write-Host "✓ Extraction complete"
```

### Step 2: Extract the Archive

The extraction will create a folder named `zig-x86_64-windows-0.15.2`

Recommended locations:
- **Option A**: In the project folder  
  `C:\Users\bonneval\source\repos\boostdas\zig-x86_64-windows-0.15.2\`
  
- **Option B**: System-wide location  
  `C:\Program Files\zig-x86_64-windows-0.15.2\`

### Step 3: Add Zig to PATH

**Temporary Method (current PowerShell session only):**

If Zig is in the project folder:
```powershell
$env:PATH = "$PWD\zig-x86_64-windows-0.15.2;$env:PATH"
```

For another location:
```powershell
$env:PATH = "C:\path\to\zig-x86_64-windows-0.15.2;$env:PATH"
```

**Permanent Method (recommended):**

1. Open System Properties:
   - Right-click "This PC" → Properties
   - Advanced system settings
   - Environment Variables

2. Edit the `Path` variable:
   - Under "System variables" or "User variables"
   - Click "Edit"
   - Add the path: `C:\Program Files\zig-x86_64-windows-0.15.2` (or your chosen location)
   - Click OK

3. **Important**: Restart PowerShell to apply changes

### Step 4: Verify Installation

Open a new PowerShell terminal and run:

```powershell
zig version
```

**Expected output:**
```
0.15.2
```

If the command fails with "zig: The term 'zig' is not recognized", verify that:
- The PATH was correctly configured
- You restarted PowerShell after modifying the PATH

## Method 2: Local Usage (without PATH)

If you prefer not to modify the PATH, you can use Zig directly:

```powershell
# Check version
.\zig-x86_64-windows-0.15.2\zig.exe version

# Build the project
.\zig-x86_64-windows-0.15.2\zig.exe build
```

## Method 3: Package Managers (Alternative)

⚠️ **Warning**: Package managers may not have the exact version 0.15.2.

**Chocolatey** (if installed):
```powershell
choco install zig --version=0.15.2
```

**Scoop** (if installed):
```powershell
scoop install zig@0.15.2
```

After installation via package manager, verify the version:
```powershell
zig version
```

## Troubleshooting

### Issue: "zig: The term 'zig' is not recognized"

**Cause**: Zig is not in the PATH.

**Solution**:
- Verify you added the correct path to PATH
- Restart PowerShell after modifying PATH
- Or use the full path: `C:\path\to\zig\zig.exe version`

### Issue: Wrong version displayed

**Cause**: Another version of Zig is installed on the system.

**Solution**:
- Check the order of paths in PATH
- Place the path to Zig 0.15.2 first
- Or uninstall other Zig versions

### Issue: Download fails or 0 byte file

**Cause**: Incorrect URL or version unavailable.

**Solution**:
- **Correct URL**: `https://ziglang.org/download/0.15.2/zig-x86_64-windows-0.15.2.zip`
- ⚠️ Note: The order is `zig-x86_64-windows-` not `zig-windows-x86_64-`
- Try downloading manually via browser if PowerShell fails
- Check your Internet connection and firewall

**Alternative**: Use the full PowerShell script in the "Step 1" section

## Next Steps

Once Zig is installed and verified, consult the [README.md](README.md) for:
- Building the project
- Running the application
- Compilation options

---

**Last Updated**: February 4, 2026  
**Required Zig Version**: 0.15.2  
**Tested On**: Windows 11 x64
