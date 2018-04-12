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
		public const byte   RefDecimal         			= 15;

		public const byte   DoubleArray					= 16;
		public const byte   DoubleList					= 17;
		public const byte   Double 						= 18;
		public const byte   DoubleDefault				= 19;
		public const byte   RefDouble   				= 20;

		public const byte   FloatArray					= 22;
		public const byte   FloatList					= 23;
		public const byte   Float 						= 24;
		public const byte   FloatDefault				= 25;
		public const byte   RefFloat    				= 26;

		public const byte   ULongArray					= 28;
		public const byte   ULongList					= 29;
		public const byte   ULong 						= 30;
		public const byte   RefULong    				= 31;

        public const byte   LongArray					= 33;
        public const byte   LongList					= 34;
        public const byte   Long 						= 35;
		public const byte   RefLong 					= 36;

		public const byte   UIntArray					= 38;
		public const byte   UIntList					= 39;
		public const byte   UInt 						= 40;

		public const byte   IntArray					= 43;
		public const byte   IntList						= 44;
		public const byte   Int 						= 45;

		public const byte   UShortArray					= 48;
		public const byte   UShortList					= 49;
		public const byte   UShort 						= 50;

		public const byte   ShortArray					= 52;
		public const byte   ShortList					= 53;
		public const byte   Short 						= 54;

		public const byte   SByteArray					= 56;
		public const byte   SByteList					= 57;
		public const byte   SByte 						= 58;
		public const byte   SByteMinusOne				= 59;

		public const byte   Byte 						= 62;
		public const byte   NumberDefault				= 63;

        //

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
		public const byte   RefGuid 					= 96;

		public const byte   DateTimeArray				= 98;
		public const byte   DateTimeList				= 99;
		public const byte   DateTime					= 100;
		public const byte   DateTimeDefault 			= 101;
		public const byte   RefDateTime 				= 102;

        public const byte   DateTimeOffsetArray			= 104;
        public const byte   DateTimeOffsetList			= 105;
        public const byte   DateTimeOffset				= 106;
		public const byte   DateTimeOffsetDefault 		= 107;
		public const byte   RefDateTimeOffset   		= 108;

		public const byte   TimeSpanArray				= 110;
		public const byte   TimeSpanList				= 111;
		public const byte   TimeSpan					= 112;
		public const byte   TimeSpanDefault    			= 113;
		public const byte   RefTimeSpan 				= 114;

        //

   		public const byte   EnumArray       			= 116;
   		public const byte   EnumList        			= 117;
		public const byte   EnumInt						= 138;

        public const byte   ByteArrayNull               = 139;
        public const byte   ByteArrayEmpty              = 140;
        public const byte   ByteArrayLength             = 143;

		public const byte   StringArray					= 144;
		public const byte   StringList 					= 145;
		public const byte   StringNull 					= 146;
        public const byte   StringEmpty                 = 147;
		public const byte   StringLength 			    = 170;

		public const byte   RefString8			    	= 171;
		public const byte   RefString16			    	= 173;
		public const byte   RefString32			    	= 175;
		public const byte   RefString64			    	= 177;
		public const byte   RefString			    	= 179;

	    public const byte   RefType					    = 181;
		public const byte   RefObject		    		= 183;

        public const byte   FileStart                 = 255;
    }
}