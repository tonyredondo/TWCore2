/*
Copyright 2015-2017 Daniel Adrian Redondo Suarez

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
 */

#pragma warning disable 1591

namespace TWCore.Serialization.WSerializer
{
    /// <summary>
    /// Serializer Byte Map Type
    /// </summary>
    public class DataType
    {
        public const byte   Unknown 					= 0;
        public const byte   TypeStart                   = 1;
        public const byte   TypeEnd                     = 2;
        public const byte   TypeRefByte                 = 3;
        public const byte   TypeRefByte0                = 4;
        public const byte   TypeRefByte1                = 5;
        public const byte   TypeRefByte2                = 6;
        public const byte   TypeRefByte3                = 7;
        public const byte   TypeRefByte4                = 8;
        public const byte   TypeRefByte5                = 9;
        public const byte   TypeRefByte6                = 10;
        public const byte   TypeRefByte7                = 11;
        public const byte   TypeRefByte8                = 12;
        public const byte   TypeRefByte9                = 13;
        public const byte   TypeRefByte10               = 14;
        public const byte   TypeRefByte11               = 15;
        public const byte   TypeRefByte12               = 16;
        public const byte   TypeRefByte13               = 17;
        public const byte   TypeRefByte14               = 18;
        public const byte   TypeRefByte15               = 19;
        public const byte   TypeRefByte16               = 20;
        public const byte   TypeRefByte17               = 21;
        public const byte   TypeRefByte18               = 22;
        public const byte   TypeRefByte19               = 23;
        public const byte   TypeRefByte20               = 24;
        public const byte   TypeRefByte21               = 25;
        public const byte   TypeRefByte22               = 26;
        public const byte   TypeRefByte23               = 27;
        public const byte   TypeRefByte24               = 28;
        public const byte   TypeRefUShort               = 29;
        public const byte   KnownType                   = 30;

        public const byte   ValueNull   				= 31;
        public const byte   ListStart                   = 32;
        public const byte   ListEnd                     = 33;
        public const byte   DictionaryStart             = 34;
        public const byte   DictionaryEnd               = 35;
        public const byte   KeyValueStart               = 36;
        public const byte   KeyValueEnd                 = 37;

        public const byte   TupleStart                  = 40;
        public const byte   TupleEnd                    = 41;

        

		public const byte   Decimal 					= 45;
		public const byte   DecimalDefault				= 46;
		public const byte   RefDecimalByte				= 47;
		public const byte   RefDecimalUShort			= 48;
		public const byte   Double 						= 49;
		public const byte   DoubleDefault				= 50;
		public const byte   RefDoubleByte				= 51;
		public const byte   RefDoubleUShort				= 52;
		public const byte   Float 						= 53;
		public const byte   FloatDefault				= 54;
		public const byte   RefFloatByte				= 55;
		public const byte   RefFloatUShort				= 56;
		public const byte   ULong 						= 57;
		public const byte   RefULongByte				= 58;
		public const byte   RefULongUShort				= 59;
        public const byte   Long 						= 60;
		public const byte   RefLongByte					= 61;
		public const byte   RefLongUShort				= 62;
		public const byte   UInt 						= 63;
		public const byte   RefUIntByte					= 64;
		public const byte   RefUIntUShort				= 65;
		public const byte   Int 						= 66;
		public const byte   RefIntByte					= 67;
		public const byte   RefIntUShort				= 68;
		public const byte   UShort 						= 69;
		public const byte   RefUShortByte				= 70;
		public const byte   Short 						= 71;
		public const byte   RefShortByte				= 72;
		public const byte   SByte 						= 73;
		public const byte   SByteMinusOne				= 74;
		public const byte   Byte 						= 75;
		public const byte   ByteDefault					= 76;
		public const byte   Byte1 					    = 77;
		public const byte   Byte2 					    = 78;
		public const byte   Byte3 					    = 79;
		public const byte   Byte4 					    = 80;
		public const byte   Byte5 					    = 81;
		public const byte   Byte6 					    = 82;
		public const byte   Byte7 					    = 83;
		public const byte   Byte8 					    = 84;
		public const byte   Byte9 					    = 85;
		public const byte   Byte10 					    = 86;
		public const byte   Byte11 					    = 87;
		public const byte   Byte12 					    = 88;
		public const byte   Byte13 					    = 89;
		public const byte   Byte14 					    = 90;
		public const byte   Byte15 					    = 91;
		public const byte   Byte16 					    = 92;
        public const byte   BoolFalse 					= 93;
        public const byte   BoolTrue                    = 94;
		public const byte   Char 						= 95;
		public const byte   CharDefault					= 96;
		public const byte   Guid						= 97;
		public const byte   GuidDefault					= 98;
		public const byte   RefGuidByte					= 99;
		public const byte   RefGuidUShort				= 100;
		public const byte   DateTime					= 101;
		public const byte   DateTimeDefault 			= 102;
		public const byte   RefDateTimeByte				= 103;
		public const byte   RefDateTimeUShort			= 104;
		public const byte   TimeSpan					= 105;
		public const byte   TimeSpanDefault    			= 106;
		public const byte   RefTimeSpanByte				= 107;
		public const byte   RefTimeSpanUShort			= 108;
        public const byte   DateTimeOffset				= 109;
        public const byte   DateTimeOffsetDefault		= 110;
		public const byte   RefDateTimeOffsetByte		= 111;
		public const byte   RefDateTimeOffsetUShort		= 112;
   		public const byte   EnumSByteMinusOne			= 113;
   		public const byte   EnumByte 					= 114;
   		public const byte   EnumByteDefault				= 115;
   		public const byte   EnumByte1   				= 116;
   		public const byte   EnumByte2   				= 117;
   		public const byte   EnumByte3   				= 118;
   		public const byte   EnumByte4   				= 119;
   		public const byte   EnumByte5   				= 120;
   		public const byte   EnumByte6   				= 121;
   		public const byte   EnumByte7   				= 122;
   		public const byte   EnumByte8   				= 123;
   		public const byte   EnumByte9   				= 124;
   		public const byte   EnumByte10   				= 125;
   		public const byte   EnumByte11   				= 126;
   		public const byte   EnumByte12   				= 127;
   		public const byte   EnumByte13   				= 128;
   		public const byte   EnumByte14   				= 129;
   		public const byte   EnumByte15   				= 130;
   		public const byte   EnumByte16   				= 131;
        public const byte   EnumUShort					= 132;
		public const byte   EnumInt						= 133;
        public const byte   ByteArrayNull               = 134;
        public const byte   ByteArrayEmpty              = 135;
        public const byte   ByteArrayLengthByte         = 136;
        public const byte   ByteArrayLengthUShort       = 137;
        public const byte   ByteArrayLengthInt          = 138;
        public const byte   RefByteArrayByte            = 139;
        public const byte   RefByteArrayUShort          = 140;

		public const byte   StringNull 					= 143;
        public const byte   StringEmpty                 = 144;
		public const byte   StringLengthByte 			= 145;
		public const byte   StringLengthByte1 			= 146;
		public const byte   StringLengthByte2 			= 147;
		public const byte   StringLengthByte3 			= 148;
		public const byte   StringLengthByte4 			= 149;
		public const byte   StringLengthByte5 			= 150;
		public const byte   StringLengthByte6 			= 151;
		public const byte   StringLengthByte7 			= 152;
		public const byte   StringLengthByte8 			= 153;
		public const byte   StringLengthByte9 			= 154;
		public const byte   StringLengthByte10 			= 155;
		public const byte   StringLengthByte11 			= 156;
		public const byte   StringLengthByte12 			= 157;
		public const byte   StringLengthByte13 			= 158;
		public const byte   StringLengthByte14 			= 159;
		public const byte   StringLengthByte15 			= 160;
		public const byte   StringLengthByte16 			= 161;
        public const byte   StringLengthUShort 			= 162;
		public const byte   StringLengthInt 			= 163;

		public const byte   RefString32Byte				= 164;
		public const byte   RefString32Byte0			= 165;
		public const byte   RefString32Byte1			= 166;
		public const byte   RefString32Byte2			= 167;
		public const byte   RefString32Byte3			= 168;
		public const byte   RefString32Byte4			= 169;
		public const byte   RefString32Byte5			= 170;
		public const byte   RefString32Byte6			= 171;
		public const byte   RefString32Byte7			= 172;
		public const byte   RefString32Byte8			= 173;
		public const byte   RefString32Byte9			= 174;
		public const byte   RefString32Byte10			= 175;
		public const byte   RefString32Byte11			= 176;
		public const byte   RefString32Byte12			= 177;
		public const byte   RefString32Byte13			= 178;
		public const byte   RefString32Byte14			= 179;
		public const byte   RefString32Byte15			= 180;
		public const byte   RefString32Byte16			= 181;
		public const byte   RefString32UShort			= 182;

		public const byte   RefString16Byte				= 183;
		public const byte   RefString16Byte0			= 184;
		public const byte   RefString16Byte1			= 185;
		public const byte   RefString16Byte2			= 186;
		public const byte   RefString16Byte3			= 187;
		public const byte   RefString16Byte4			= 188;
		public const byte   RefString16Byte5			= 189;
		public const byte   RefString16Byte6			= 190;
		public const byte   RefString16Byte7			= 191;
		public const byte   RefString16Byte8			= 192;
		public const byte   RefString16Byte9			= 193;
		public const byte   RefString16Byte10			= 194;
		public const byte   RefString16Byte11			= 195;
		public const byte   RefString16Byte12			= 196;
		public const byte   RefString16Byte13			= 197;
		public const byte   RefString16Byte14			= 198;
		public const byte   RefString16Byte15			= 199;
		public const byte   RefString16Byte16			= 200;
		public const byte   RefString16UShort			= 201;

		public const byte   RefStringByte				= 202;
		public const byte   RefStringUShort	    		= 203;

		public const byte   RefObjectByte				= 204;
		public const byte   RefObjectByte0				= 205;
		public const byte   RefObjectByte1				= 206;
		public const byte   RefObjectByte2				= 207;
		public const byte   RefObjectByte3				= 208;
		public const byte   RefObjectByte4				= 209;
		public const byte   RefObjectByte5				= 210;
		public const byte   RefObjectByte6				= 211;
		public const byte   RefObjectByte7				= 212;
		public const byte   RefObjectByte8				= 213;
		public const byte   RefObjectByte9				= 214;
		public const byte   RefObjectByte10				= 215;
		public const byte   RefObjectByte11				= 216;
		public const byte   RefObjectByte12				= 217;
		public const byte   RefObjectByte13				= 218;
		public const byte   RefObjectByte14				= 219;
		public const byte   RefObjectByte15				= 220;
		public const byte   RefObjectByte16				= 221;
        public const byte   RefObjectUShort				= 222;

        public const byte   FileStart                   = 225;
        public const byte   FileStartCompressed         = 226;

        public const byte   TypeNameDecimal             = 233;
        public const byte   TypeNameDouble              = 234;
        public const byte   TypeNameFloat               = 235;
        public const byte   TypeNameULong               = 236;
        public const byte   TypeNameLong                = 237;
        public const byte   TypeNameUInt                = 238;
        public const byte   TypeNameInt                 = 239;
        public const byte   TypeNameUShort              = 240;
        public const byte   TypeNameShort               = 241;
        public const byte   TypeNameSByte               = 242;
        public const byte   TypeNameByte                = 243;
        public const byte   TypeNameBool                = 244;
        public const byte   TypeNameChar                = 245;
        public const byte   TypeNameGuid                = 246;
        public const byte   TypeNameDateTime            = 247;
        public const byte   TypeNameTimeSpan            = 248;
        public const byte   TypeNameDateTimeOffset      = 249;
        public const byte   TypeNameString              = 250;

        public const byte   WDataCachedUShort           = 251;
        public const byte   WDataCached2048             = 252;
        public const byte   WDataCached1024             = 253;
        public const byte   WDataCached512              = 254;
        public const byte   WDataNoCached               = 255;
    }
}
