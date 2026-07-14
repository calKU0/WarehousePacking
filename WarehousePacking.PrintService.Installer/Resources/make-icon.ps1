# One-off helper: builds a multi-resolution .ico from the PWA manifest icon.
# Not part of the build - run manually if the source icon ever changes.
Add-Type -AssemblyName System.Drawing

$sourcePath = "C:\Users\krzys\Desktop\Projects\KontrolaPakowania\WarehousePacking.Server\wwwroot\images\icons\icon-512.png"
$outPath = "C:\Users\krzys\Desktop\Projects\KontrolaPakowania\WarehousePacking.PrintService.Installer\Resources\KontrolaPakowania.ico"
$sizes = @(256, 128, 64, 48, 32, 16)

$src = [System.Drawing.Image]::FromFile($sourcePath)

$pngBlobs = @()
foreach ($size in $sizes) {
    $bmp = New-Object System.Drawing.Bitmap $size, $size
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
    $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $g.DrawImage($src, 0, 0, $size, $size)
    $g.Dispose()

    $ms = New-Object System.IO.MemoryStream
    $bmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
    $pngBlobs += ,($size, $ms.ToArray())
    $bmp.Dispose()
}
$src.Dispose()

$fs = [System.IO.File]::Open($outPath, [System.IO.FileMode]::Create)
$bw = New-Object System.IO.BinaryWriter $fs

# ICONDIR
$bw.Write([UInt16]0)      # reserved
$bw.Write([UInt16]1)      # type = icon
$bw.Write([UInt16]$pngBlobs.Count)

$headerSize = 6
$entrySize = 16
$offset = $headerSize + ($entrySize * $pngBlobs.Count)

foreach ($entry in $pngBlobs) {
    $size = $entry[0]
    $bytes = $entry[1]
    $dim = if ($size -ge 256) { 0 } else { $size }
    $bw.Write([byte]$dim)         # width
    $bw.Write([byte]$dim)         # height
    $bw.Write([byte]0)            # color palette
    $bw.Write([byte]0)            # reserved
    $bw.Write([UInt16]1)          # color planes
    $bw.Write([UInt16]32)         # bits per pixel
    $bw.Write([UInt32]$bytes.Length)
    $bw.Write([UInt32]$offset)
    $offset += $bytes.Length
}

foreach ($entry in $pngBlobs) {
    $bw.Write([byte[]]$entry[1])
}

$bw.Flush()
$bw.Close()
$fs.Close()

Write-Output "Wrote $outPath"
