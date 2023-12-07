# DVPL Converter on C#
It is representation of Maddoxkkm's converter on Node.JS.
https://github.com/Maddoxkkm/dvpl_converter

# Requires
1. .NET Framework >= 4.5
2. K4os.Compression.LZ4
3. Force.Crc32

# How To Use
There are 3 methods and 1 class that might need you.

Let's start with what everybody is looking for.

## `decompressDVPL`
```cs
using DVPLConverter;

DVPL DVPLConv = new DVPL();

try{
  byte[] decompressed = DVPLConv.decompressDVPL(File.ReadAllBytes(@"C:\file.txt.dvpl"));
  File.WriteAllBytes(@"C:\file.txt", decompressed);
}catch{ Console.WriteLine("Throwed an exception. Check your file.") }
```

## `compressDVPL`
On one hand, it's simple.
```cs
byte[] compressed = DVPLConv.compressDVPL(File.ReadAllBytes(@"C:\file.txt"));
```
On the other hand, original .tex files don't use compression (their compression level is 0), but other files do (their compression level is 2).
Safest use (in my humble opinion) is to use an overload of the method
```cs
byte[] compressed = DVPLConv.compressDVPL(File.ReadAllBytes(path), path.EndsWith(".tex"));
```

## Class `DVPLFooterData` 
It consists of four elements, which are included into any DVPL file:
1. Original size of file - `oSize`
2. Compressed size of file - `cSize`
3. CRC32 of compressed file without footer - `crc32`
4. Type (level) of compression - `type`

They are all ***uint***.

## `readDVPLFooter`
```cs
DVPLFooterData info = DVPLConv.readDVPLFooter(File.ReadAllBytes(@"C:\file.txt.dvpl"))
```
