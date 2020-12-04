ASCENSION_IS.exe:0054D89D

DED intro magic bytes xoring
```
3C->82
32->(7b)->49
E7->(c5)->22
E2->(79)->9B
```
====================

* functions using DER encoding internally:
```
DemiseView.dll:
 DEVSetBitmapDir
 DEVSetTextureDir
 DEVSetObjectDir

 DEVLoadBitmap

 .text:0E843400 - DES_read
 .text:0E843010 - DES_write
 .text:0E863340 - XOR_decoder (see below)
DemiseSound.dll:
 DESSetSoundDir
 DESPlay
0x49CB878
```
DER structure:
```
 // front part
 char magic[8]; // 'DERv1.3 '
 int32 tailOffset;
 struct {
  byte xoredZlibStream[dataLen];
 }[count];
 // tail part
 int32 count;
 int32 _unused;
 struct {
  int32 strLen;
  char str[strLen]; // (XOR masked)
  int32 dataOffset;
  int32 dataLen;
  int32 _unknown;
 }[count];
 // EOF
```
```
rol = (x, shift) => ( x << shift ) | ( x >>> ( 32 - shift ) );
```
====================

zlib stream XOR:
```
str = 'intro.des' // asset name, str2 = 'DEMISESOUNDFX' (fname caps no ext)
xor0 = (str) => {
    esi = 0x00091C80; // esi = 0x000A2F3B;
    ecx = esi;
    for( i = 0; i < str.length; i++ ) {
        rol( ecx, 3 );
        //ecx ^= str[i];
        ecx ^= str.charCodeAt(i);
        ecx ^= esi;
        ecx ^= 0xABCDABCD;
        esi = ecx;
    }
    return esi;
};
// str1 (asset name 'intro.des') -> 0xFB344021
// str2 (DER NAME CAPS NO EXT 'DEMISESOUNDFX')-> 0xD800FB0F
// mul = strHash1 * strHash2 // 0xCAF01CEF
// mul += 4
// mul ^= 0x00019871 // CAF18482
// -> [E435CD0] ->
for( i = 0; i < 0x200; i++ ) {
// mul *= 0x0343FD; // 53E3FA7A
// mul += 0x0269EC3; // 540A993D // 5[40]A993D -> al
// mul -> [E435CD0]
  mul <<= 9;
  mul >>= 30; // 16+4+1
  // mul &= 0x7FFF; // mul >>= 4; 
  xorMask[i] = al;
}
return xorMask;

// ESI = 0x5d initially for whatever reason;
// ECX is const XOR table from arg (ca. 084B0260), 0x200 long
// eax === edi (both ++), ARG_4 is len (--)
xor1 = (str) => {
    ret = '';
    esi = 0x5D;
    for( i = 0; i < stream.length; i++ ) {
        edx = esi & 0x01ff; // take only 9 lowest bits
        ret += String.fromCharCode( str.charCodeAt(i) ^ ECX[edx] );        
        //stream[i] ^= ECX[edx];
        esi += 0x000D6543;
        edx = ( esi >> 3 ) & 0x01FF;
        esi ^= ECX[edx];
    }
  return ret;
};

ECX =
A3 E4 3E EE 3B EF 5D 7D  9E D9 8E C1 8E B4 C5 AE
BC 24 F1 75 F3 16 FE 5C  4F E5 2E AF 80 C9 B8 D4
B8 58 B0 ED A9 92 1A 29  1B 04 52 78 34 5E 46 02
F1 66 1A 44 CC 25 5E 22  43 BA 06 08 90 B3 62 F6
8A 5A 61 C4 49 92 D4 95  88 AD 3B CD 45 39 B2 12
A5 00 73 0A 0E C3 79 73  42 CA 7C 84 42 E1 BA 06
76 63 09 3F 89 56 9E 6F  F2 22 12 53 B8 C4 21 5B
A6 E0 94 61 3F AA 39 5A  E8 80 C9 D7 6C BA 7B 60
A9 32 A4 12 DE C5 F2 D9  87 33 A9 D6 59 BE 2F 57
78 A7 6F 36 2D 28 06 E4  94 D9 17 E4 FC 4F D7 EE
D1 4F 79 8F F7 0F 98 0F  FC A8 92 F0 27 90 6B 41
F3 AA 0E 6B 34 8C 83 6B  23 FD 4F 36 97 77 86 3E
DB B1 C2 6C C7 8C 62 AD  DE A0 D5 3E 5E 47 A8 9E
52 13 1B 9D AA D8 D1 64  E3 5C A2 32 13 09 43 71
65 A0 BA 02 3E BB CA AD  14 EA 45 8A C5 85 4F 61
24 BF AE 3F 55 98 62 71  70 EE 03 92 D4 24 7E E4
64 A3 E1 C6 11 6C 59 F4  2A CE F3 29 21 90 61 CD
3D 08 29 E4 12 57 56 96  F0 D9 1A 8B CA 61 B2 BA
05 A9 F9 82 B4 DE 59 F5  8E 9C E0 B6 77 21 00 DE
5D DC 6E 47 EF 52 90 D8  BA 55 D9 4F 08 57 10 07
68 43 03 34 DE 28 13 1B  3E F3 AE 7C 3D 6D C7 4F
97 DB 16 75 B9 99 40 D0  62 D2 72 92 3D D4 18 A5
47 27 32 ED FB 03 81 4A  B0 D3 1E 9F C1 AE 3D 54
AA 15 7C E1 8D B5 89 0B  06 78 CE C8 46 C2 FE BA
7B 96 4E 2A 7A 85 B4 B6  AF 92 82 E3 C7 5A 04 38
2B 39 8F AD FA 2C CC C4  C3 C5 9F 73 C0 2F B2 DF
F7 56 91 ED 51 66 B5 8A  63 41 E4 62 05 29 B7 30
5F 65 9A FF 8D 4D 7B 1F  EF A2 CD 71 9A 32 2E A9
E1 20 BC FC 7B 46 C7 71  09 37 5C 71 49 5B 9E 7D
CD 3B C6 B1 23 19 6E 57  20 7D CC 84 E9 E4 EB C3
6F C2 12 60 58 8C 2A 5F  26 B4 75 EA 11 1F 14 95
88 11 ED F6 41 1F D0 38  42 CF 53 67 99 6B 5B 78
```
--------------------
```
.text:0E8436AE call sub_E863340 ; XOR tail part decoder
```

--------------------
```
xor2 = (str) => {
    ret = '';
    ebx = 0x01EF;
    for( i = 0; i < str.length; i++ ) {
        ebx ^= 0x0AE4D5DCD;
        ecx = ebx;
        ecx = ( ecx << 3 ) | ( ecx >>> 32 - 3 ); // rotateLeft( ecx, 3 );
        ebx ^= ecx;
        ret += String.fromCharCode( str.charCodeAt(i) ^ ( ebx & 0xFF ) );
    }
    return ret;
};
```
--------------------
```
mask = [ "37", "29", "c6", "57", "4d", "82", "31", "18", "7c", "38", "5d", "11", "3a", "4c", "88", "6b", "94", "90", "b7", "aa", "5a", "2e", "fb", "87", "18", "7b", "3", "b8", "de", "8b", /* 30 distinct vals, and then... */ "74", "74", AD_NAUSEAM_74 ] // VERIFIED
```
--------------------

explanation:
```
ebx = 0x1B84F674 = 0b 000110111000010011110110 01110100
      0xAE4D5DCD = 0b 101011100100110101011101 11001101
ebx = XOR        = 0b 101101011100100110101011 10111001
ecx = rol ebx, 3 = 0b 101011100100110101011101 11001101
:D tl;dr
ror( 0xCD, 3 ) ^ 0xCD = 0x74 // possible since 3 MSB (101) in 3rd val are equal to its bits 8-6 anyway
```
--------------------