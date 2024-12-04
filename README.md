# OFXSharper

OFXSharper is a .Net Standard version of James Hollingworth's popular OFX Parser, OFXSharper (https://github.com/jhollingworth/OFXSharp).

This is pretty much the same version that he last committed, with a few bug fixes (eg: Available Balance, if present, wasn't properly being returned).

## Sample Usage

```c#
var parser = new OFXDocumentParser();
var ofxDocument = parser.Import(new FileStream(@"c:\ofxdoc.ofx", FileMode.Open));
```
