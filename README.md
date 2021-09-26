# DVPL Converter on C#
It is representation of Maddoxkkm's converter on Node.JS.
https://github.com/Maddoxkkm/dvpl_converter

# How-To-Use
There are 3 methods and 1 class that might need you.

Let's start with what everybody is looking for.

## `decompressDVPL`
It is meant to be used, for example, like this:
```cs
try{
  byte[] decompressed = DVPL.decompressDVPL(File.ReadAllBytes(@"C:\file.txt.dvpl"));
  File.WriteAllBytes(@"C:\file.txt", decompressed);
}catch{ Console.WriteLine("exception lol") }
```
As there is a lot of throws in my code, it's better to put it in try...catch everytime lol.

## `compressDVPL`
There are two variants of method.
Firstly, originally .tex files don't use compression (their compression level is 0), but other files do (their compression level is 2).
That's why if you want to use this method for compressing **exact files**, you should use this:
```cs
byte[] compressed = DVPL.compressDVPL(File.ReadAllBytes(@"C:\file.txt"), ".txt");
```
As you see, there is a second argument, which is used to specify extension of file. If you type **".tex"**, it will set compression level to 0 for this file.
But if you are compressing, for example, other buffers of data, or you won't use it for .tex files, you can use simplified version:
```cs
byte[] compressed = DVPL.compressDVPL(File.ReadAllBytes(@"C:\file.txt"));
```
In that case compression level is always set to 2.

## Class `DVPLFooterData` 
It consists of four elements, which are included into any DVPL file:
1. Original size of file - `oSize`
2. Compressed size of file - `cSize`
3. CRC32 of original file - `crc32`
4. Type (level) of compression - `type`

They are all ***uint***.

## `readDVPLFooter`
If you just want to read DVPL file's footer, use this:
```cs
DVPLFooterData info = DVPL.readDVPLFooter(File.ReadAllBytes(@"C:\file.txt.dvpl"))
```
I think you can figure out what to do next by yourself.
#
It have not been tested on not-text files yet as I am busy now with some other projects. So, if there will be problems with non-text files, I will fix them, but it will take some time.

Contact me through Discord if you need smth: Endg4me_#7769
