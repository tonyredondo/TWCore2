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

namespace TWCore.Serialization.RawSerializer
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

        public const byte   DoubleArray					= 15;
		public const byte   DoubleList					= 16;
		public const byte   Double 						= 17;
		public const byte   DoubleDefault				= 18;

        public const byte   FloatArray					= 20;
		public const byte   FloatList					= 21;
		public const byte   Float 						= 22;
		public const byte   FloatDefault				= 23;

        public const byte   ULongArray					= 25;
		public const byte   ULongList					= 26;
		public const byte   ULong 						= 27;
		public const byte   ULongDefault 				= 28;

        public const byte   LongArray					= 30;
        public const byte   LongList					= 31;
        public const byte   Long 						= 32;
        public const byte   LongDefault 				= 33;

		public const byte   UIntArray					= 35;
		public const byte   UIntList					= 36;
		public const byte   UInt 						= 37;
		public const byte   UIntDefault 				= 38;

		public const byte   IntArray					= 39;
		public const byte   IntList						= 40;
		public const byte   Int 						= 41;
		public const byte   IntDefault 					= 42;

		public const byte   UShortArray					= 43;
		public const byte   UShortList					= 44;
		public const byte   UShort 						= 45;
		public const byte   UShortDefault 				= 46;

		public const byte   ShortArray					= 47;
		public const byte   ShortList					= 48;
		public const byte   Short 						= 49;
		public const byte   ShortDefault 				= 50;

		public const byte   SByteArray					= 51;
		public const byte   SByteList					= 52;
		public const byte   SByte 						= 53;
		public const byte   SByteDefault 				= 54;
		public const byte   SByteMinusOne				= 55;

		public const byte   Byte 						= 56;
		public const byte   ByteDefault	    			= 57;

        public const byte   BoolArray 					= 58;
        public const byte   BoolList 					= 59;
        public const byte   BoolFalse 					= 60;
        public const byte   BoolTrue                    = 61;
		public const byte   CharArray					= 62;
		public const byte   CharList					= 63;
		public const byte   Char 						= 64;
		public const byte   CharDefault					= 65;
		public const byte   GuidArray					= 66;
		public const byte   GuidList					= 67;
		public const byte   Guid						= 68;
		public const byte   GuidDefault					= 69;
		public const byte   DateTimeArray				= 71;
		public const byte   DateTimeList				= 72;
		public const byte   DateTime					= 73;
		public const byte   DateTimeDefault 			= 74;
        public const byte   DateTimeOffsetArray			= 76;
        public const byte   DateTimeOffsetList			= 77;
        public const byte   DateTimeOffset				= 78;
		public const byte   DateTimeOffsetDefault 		= 79;
		public const byte   TimeSpanArray				= 81;
		public const byte   TimeSpanList				= 82;
		public const byte   TimeSpan					= 83;
		public const byte   TimeSpanDefault    			= 84;
   		public const byte   EnumArray       			= 86;
   		public const byte   EnumList        			= 87;
		public const byte   EnumInt						= 88;
        public const byte   ByteArrayNull               = 89;
        public const byte   ByteArrayEmpty              = 90;
        public const byte   ByteArrayLength             = 91;
		public const byte   StringArray					= 92;
		public const byte   StringList 					= 93;
		public const byte   StringNull 					= 94;
        public const byte   StringEmpty                 = 95;
		public const byte   StringLength 			    = 96;
	    public const byte   RefType					    = 101;
		public const byte   RefObject		    		= 102;

        
        public const byte   Start                       = 252;
        public const byte   End                         = 253;
    }
}