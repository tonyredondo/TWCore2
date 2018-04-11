/*
Copyright 2015-2018 Daniel Adrian Redondo Suarez

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

namespace TWCore.Serialization.NSerializer
{
    /// <summary>
    /// Serializer Bytes Definition
    /// </summary>
    public class DataBytesDefinition
    {
        public const byte   Unknown 					= 0;
        public const byte   ValueNull   				= 1;
        public const byte   TypeStart                   = 2;
        public const byte   TypeEnd                     = 3;
        public const byte   ArrayStart                  = 4;
        public const byte   ListStart                   = 5;
        public const byte   DictionaryStart             = 6;
        public const byte   PropertiesStart             = 7;
	    public const byte	SerializedObject			= 8;
	    public const byte	SerializedObjectNull		= 9;
	    
		public const byte   DecimalArray				= 10;
		public const byte   DecimalList 				= 11;
		public const byte   Decimal 					= 12;
		public const byte   DecimalDefault				= 13;
		public const byte   RefDecimalByte				= 14;
		public const byte   RefDecimalUShort			= 15;

		public const byte   DoubleArray					= 16;
		public const byte   DoubleList					= 17;
		public const byte   Double 						= 18;
		public const byte   DoubleDefault				= 19;
		public const byte   RefDoubleByte				= 20;
		public const byte   RefDoubleUShort				= 21;

		public const byte   FloatArray					= 22;
		public const byte   FloatList					= 23;
		public const byte   Float 						= 24;
		public const byte   FloatDefault				= 25;
		public const byte   RefFloatByte				= 26;
		public const byte   RefFloatUShort				= 27;

		public const byte   ULongArray					= 28;
		public const byte   ULongList					= 29;
		public const byte   ULong 						= 30;
		public const byte   RefULongByte				= 31;
		public const byte   RefULongUShort				= 32;

        public const byte   LongArray					= 33;
        public const byte   LongList					= 34;
        public const byte   Long 						= 35;
		public const byte   RefLongByte					= 36;
		public const byte   RefLongUShort				= 37;

		public const byte   UIntArray					= 38;
		public const byte   UIntList					= 39;
		public const byte   UInt 						= 40;
		public const byte   RefUIntByte					= 41;
		public const byte   RefUIntUShort				= 42;

		public const byte   IntArray					= 43;
		public const byte   IntList						= 44;
		public const byte   Int 						= 45;
		public const byte   RefIntByte					= 46;
		public const byte   RefIntUShort				= 47;

		public const byte   UShortArray					= 48;
		public const byte   UShortList					= 49;
		public const byte   UShort 						= 50;
		public const byte   RefUShortByte				= 51;

		public const byte   ShortArray					= 52;
		public const byte   ShortList					= 53;
		public const byte   Short 						= 54;
		public const byte   RefShortByte				= 55;

		public const byte   SByteArray					= 56;
		public const byte   SByteList					= 57;
		public const byte   SByte 						= 58;
		public const byte   SByteMinusOne				= 59;

		public const byte   Byte 						= 62;
		public const byte   ByteDefault					= 63;
		public const byte   Byte1 					    = 64;
		public const byte   Byte2 					    = 65;
		public const byte   Byte3 					    = 66;
		public const byte   Byte4 					    = 67;
		public const byte   Byte5 					    = 68;
		public const byte   Byte6 					    = 69;
		public const byte   Byte7 					    = 70;
		public const byte   Byte8 					    = 71;
		public const byte   Byte9 					    = 72;
		public const byte   Byte10 					    = 73;
		public const byte   Byte11 					    = 74;
		public const byte   Byte12 					    = 75;
		public const byte   Byte13 					    = 76;
		public const byte   Byte14 					    = 77;
		public const byte   Byte15 					    = 78;
		public const byte   Byte16 					    = 79;
        public const byte   Byte17 					    = 80;
        public const byte   Byte18 					    = 81;
        public const byte   Byte19 					    = 82;
        public const byte   Byte20 					    = 83;

        public const byte   BoolArray 					= 84;
        public const byte   BoolList 					= 85;
        public const byte   BoolFalse 					= 86;
        public const byte   BoolTrue                    = 87;

		public const byte   CharArray					= 88;
		public const byte   CharList					= 89;
		public const byte   Char 						= 90;
		public const byte   CharDefault					= 91;

		public const byte   GuidArray					= 92;
		public const byte   GuidList					= 93;
		public const byte   Guid						= 94;
		public const byte   GuidDefault					= 95;
		public const byte   RefGuidByte					= 96;
		public const byte   RefGuidUShort				= 97;

		public const byte   DateTimeArray				= 98;
		public const byte   DateTimeList				= 99;
		public const byte   DateTime					= 100;
		public const byte   DateTimeDefault 			= 101;
		public const byte   RefDateTimeByte				= 102;
		public const byte   RefDateTimeUShort			= 103;

        public const byte   DateTimeOffsetArray			= 104;
        public const byte   DateTimeOffsetList			= 105;
        public const byte   DateTimeOffset				= 106;
		public const byte   DateTimeOffsetDefault 		= 107;
		public const byte   RefDateTimeOffsetByte		= 108;
		public const byte   RefDateTimeOffsetUShort		= 109;

		public const byte   TimeSpanArray				= 110;
		public const byte   TimeSpanList				= 111;
		public const byte   TimeSpan					= 112;
		public const byte   TimeSpanDefault    			= 113;
		public const byte   RefTimeSpanByte				= 114;
		public const byte   RefTimeSpanUShort			= 115;

   		public const byte   EnumArray       			= 116;
   		public const byte   EnumList        			= 117;
   		public const byte   EnumSByteMinusOne			= 118;
   		public const byte   EnumByte 					= 119;
   		public const byte   EnumByteDefault				= 120;
   		public const byte   EnumByte1   				= 121;
   		public const byte   EnumByte2   				= 122;
   		public const byte   EnumByte3   				= 123;
   		public const byte   EnumByte4   				= 124;
   		public const byte   EnumByte5   				= 125;
   		public const byte   EnumByte6   				= 126;
   		public const byte   EnumByte7   				= 127;
   		public const byte   EnumByte8   				= 128;
   		public const byte   EnumByte9   				= 129;
   		public const byte   EnumByte10   				= 130;
   		public const byte   EnumByte11   				= 131;
   		public const byte   EnumByte12   				= 132;
   		public const byte   EnumByte13   				= 133;
   		public const byte   EnumByte14   				= 134;
   		public const byte   EnumByte15   				= 135;
   		public const byte   EnumByte16   				= 136;
        public const byte   EnumUShort					= 137;
		public const byte   EnumInt						= 138;

        public const byte   ByteArrayNull               = 139;
        public const byte   ByteArrayEmpty              = 140;
        public const byte   ByteArrayLengthByte         = 141;
        public const byte   ByteArrayLengthUShort       = 142;
        public const byte   ByteArrayLengthInt          = 143;

		public const byte   StringArray					= 144;
		public const byte   StringList 					= 145;
		public const byte   StringNull 					= 146;
        public const byte   StringEmpty                 = 147;
		public const byte   StringLengthByte 			= 148;
		public const byte   StringLengthByte1 			= 149;
		public const byte   StringLengthByte2 			= 150;
		public const byte   StringLengthByte3 			= 151;
		public const byte   StringLengthByte4 			= 152;
		public const byte   StringLengthByte5 			= 153;
		public const byte   StringLengthByte6 			= 154;
		public const byte   StringLengthByte7 			= 155;
		public const byte   StringLengthByte8 			= 156;
		public const byte   StringLengthByte9 			= 157;
		public const byte   StringLengthByte10 			= 158;
		public const byte   StringLengthByte11 			= 159;
		public const byte   StringLengthByte12 			= 160;
		public const byte   StringLengthByte13 			= 161;
		public const byte   StringLengthByte14 			= 162;
		public const byte   StringLengthByte15 			= 163;
		public const byte   StringLengthByte16 			= 164;
		public const byte   StringLengthByte17 			= 165;
		public const byte   StringLengthByte18 			= 166;
		public const byte   StringLengthByte19 			= 167;
		public const byte   StringLengthByte20 			= 168;
        public const byte   StringLengthUShort 			= 169;
		public const byte   StringLengthInt 			= 170;

		public const byte   RefString8Byte				= 171;
		public const byte   RefString8UShort			= 172;
		public const byte   RefString16Byte				= 173;
		public const byte   RefString16UShort			= 174;
		public const byte   RefString32Byte				= 175;
		public const byte   RefString32UShort			= 176;
		public const byte   RefString64Byte				= 177;
		public const byte   RefString64UShort			= 178;
		public const byte   RefStringByte				= 179;
		public const byte   RefStringUShort		    	= 180;

		public const byte   RefObjectByte				= 224;
		public const byte   RefObjectByte0				= 225;
		public const byte   RefObjectByte1				= 226;
		public const byte   RefObjectByte2				= 227;
		public const byte   RefObjectByte3				= 228;
		public const byte   RefObjectByte4				= 229;
		public const byte   RefObjectByte5				= 230;
		public const byte   RefObjectByte6				= 231;
		public const byte   RefObjectByte7				= 232;
		public const byte   RefObjectByte8				= 233;
		public const byte   RefObjectByte9				= 234;
		public const byte   RefObjectByte10				= 235;
		public const byte   RefObjectByte11				= 236;
		public const byte   RefObjectByte12				= 237;
		public const byte   RefObjectByte13				= 238;
		public const byte   RefObjectByte14				= 239;
		public const byte   RefObjectByte15				= 240;
		public const byte   RefObjectByte16				= 241;
		public const byte   RefObjectByte17				= 242;
		public const byte   RefObjectByte18				= 243;
		public const byte   RefObjectByte19				= 244;
		public const byte   RefObjectByte20				= 245;
        public const byte   RefObjectUShort				= 246;

        public const byte   FileStart                 = 255;
    }
}