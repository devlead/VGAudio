﻿using System;
using System.Linq;
using VGAudio.Utilities;

namespace VGAudio.Codecs.CriHca
{
    public static class CriHcaTables
    {
        static CriHcaTables()
        {
            Array[] arrays = ArrayUnpacker.UnpackArrays(PackedTables);
            QuantizeSpectrumBits = (byte[][])arrays[0];
            QuantizeSpectrumValue = (byte[][])arrays[1];
            QuantizedSpectrumBits = (byte[][])arrays[2];
            QuantizedSpectrumMaxBits = (byte[])arrays[3];
            QuantizedSpectrumValue = (sbyte[][])arrays[4];
            ScaleToResolutionCurve = (byte[])arrays[5];
            AthCurve = (byte[])arrays[6];
            MdctWindow = ((float[])arrays[7]).Select(x => (double)x).ToArray();
        }

        public static float[] DequantizerScalingTable { get; } = Arrays.Generate(64, DequantizerScalingFunction);
        public static float[] DequantizerNormalizeTable { get; } = Arrays.Generate(16, DequantizerNormalizeFunction);
        public static float[] QuantizerScalingTable { get; } = Arrays.Generate(64, i => 1 / DequantizerScalingFunction(i));
        public static float[] QuantizerRangeTable { get; } = Arrays.Generate(16, QuantizerRangeFunction);
        public static int[] ResolutionLevelsTable { get; } = Arrays.Generate(16, ResolutionLevelsFunction);
        public static float[] IntensityRatioTable { get; } = Arrays.Generate(16, IntensityRatioFunction);
        public static float[] ScaleConversionTable { get; } = Arrays.Generate(128, ScaleConversionTableFunction);

        public static byte[] ScaleToResolutionCurve { get; }
        public static byte[] QuantizedSpectrumMaxBits { get; }
        public static byte[][] QuantizedSpectrumBits { get; }
        public static sbyte[][] QuantizedSpectrumValue { get; }
        public static byte[][] QuantizeSpectrumBits { get; }
        public static byte[][] QuantizeSpectrumValue { get; }

        // Don't know what the window function is.
        // It's close to a KBD window with an alpha of around 3.82.
        // AAC and Vorbis windows are similar to it.
        public static double[] MdctWindow { get; }

        /// <summary>
        /// Represents an Absolute Threshold of Hearing (ATH) curve. 
        /// This curve is used when deriving resolutions from scale factors in very old HCA versions.
        /// </summary>
        /// <seealso cref="CriHcaFrame.ScaleAthCurve"/>
        /// <remarks>This curve seems to be a slight modification of the standard Painter & Spanias ATH curve formula</remarks>
        public static byte[] AthCurve { get; }

        private static float DequantizerScalingFunction(int x) => (float)(Math.Sqrt(128) * Math.Pow(Math.Pow(2, 53f / 128), x - 63));
        private static float ScaleConversionTableFunction(int x) => x > 1 && x < 127 ? (float)Math.Pow(Math.Pow(2, 53f / 128), x - 64) : 0;
        private static float IntensityRatioFunction(int x) => x <= 14 ? (14 - x) / 7f : 0;

        private static float DequantizerNormalizeFunction(int x)
        {
            if (x == 0) return 0;
            if (x < 8) return 2f / (2 * x + 1);
            return 2f / ((1 << (x - 3)) - 1);
        }

        private static float QuantizerRangeFunction(int x) => ResolutionLevelsFunction(x) / 2f;

        private static int ResolutionLevelsFunction(int x)
        {
            if (x < 8) return 2 * x + 1;
            return (1 << (x - 3)) - 1;
        }

        public static readonly byte[] PackedTables =
        {
            0x01, 0xFF, 0x06, 0x00, 0x00, 0x74, 0xCF, 0x85, 0x71, 0x03, 0x51, 0x0C, 0x84, 0x61, 0x69, 0xA5,
            0x63, 0x9C, 0x14, 0x91, 0x26, 0xD2, 0x5C, 0x1A, 0x08, 0x75, 0x6B, 0xA6, 0xD5, 0x1B, 0x47, 0x01,
            0xFF, 0xF6, 0xC1, 0xA7, 0x63, 0x91, 0x67, 0x69, 0x05, 0xAD, 0xAC, 0x69, 0xA5, 0xAB, 0x3C, 0x71,
            0xFB, 0x22, 0xFA, 0x8E, 0xAF, 0x57, 0x79, 0x10, 0x14, 0x99, 0x06, 0xC0, 0x12, 0xCD, 0x60, 0x76,
            0x1F, 0xB8, 0x45, 0x5E, 0xE8, 0x1E, 0x74, 0x0F, 0xC6, 0x80, 0x5B, 0x92, 0x03, 0x8F, 0xE4, 0xFF,
            0x4C, 0x90, 0xD9, 0x40, 0xB4, 0x4E, 0xAC, 0x4C, 0xE0, 0xF7, 0xC1, 0x52, 0x3B, 0x44, 0xAD, 0x9A,
            0x0B, 0xA7, 0x81, 0xEE, 0xC7, 0xB9, 0xB0, 0xE3, 0xF5, 0x2D, 0x07, 0x24, 0xAF, 0x8F, 0xC1, 0xAF,
            0x54, 0x81, 0x6C, 0x30, 0xB3, 0x6F, 0x1B, 0x93, 0x92, 0xE5, 0xCC, 0x3D, 0xED, 0xB3, 0xBC, 0x1F,
            0xF6, 0x9C, 0x0B, 0x62, 0x52, 0xD5, 0x4D, 0xDB, 0xF5, 0xC3, 0x28, 0x7F, 0xD2, 0xF3, 0x2F, 0xEB,
            0xF9, 0x8C, 0x53, 0xF2, 0x55, 0x76, 0xFC, 0x75, 0x1C, 0xA7, 0x93, 0xD9, 0xF1, 0xE8, 0x87, 0xE4,
            0xAB, 0xAA, 0x7D, 0x71, 0x51, 0xBD, 0xBB, 0x3A, 0xA9, 0xD9, 0x2E, 0x33, 0x9B, 0xD8, 0xC8, 0x06,
            0xD6, 0x47, 0x1D, 0x6B, 0x59, 0x53, 0xD7, 0x55, 0x7C, 0x22, 0xAE, 0xE9, 0x85, 0x30, 0x78, 0x30,
            0x60, 0x20, 0x8A, 0x02, 0x00, 0x36, 0x58, 0xCF, 0xB6, 0xAD, 0xBF, 0x46, 0xC7, 0xEF, 0x53, 0xAD,
            0xE4, 0xAC, 0x86, 0xBA, 0xC8, 0xD2, 0x38, 0x8A, 0xC2, 0x20, 0xF0, 0x7D, 0xDF, 0x03, 0x2E, 0x71,
            0x84, 0x7D, 0x63, 0x5D, 0x99, 0x4F, 0x0C, 0xA2, 0x33, 0x4D, 0x9C, 0xFE, 0xD0, 0x84, 0x2E, 0x0C,
            0x66, 0x7E, 0x62, 0x7D, 0x62, 0xFF, 0xE4, 0x7C, 0xE4, 0xBE, 0xF2, 0x9E, 0xF8, 0x0F, 0x82, 0x9B,
            0xF0, 0x26, 0x12, 0xB1, 0x48, 0x44, 0xCA, 0x32, 0x92, 0x93, 0x82, 0x94, 0xA4, 0x42, 0x35, 0x6A,
            0x50, 0x0B, 0x3A, 0xD4, 0x83, 0x01, 0x8C, 0x68, 0x9A, 0xA6, 0x19, 0x2C, 0x60, 0x05, 0xDB, 0xB6,
            0xED, 0xE0, 0x38, 0x0E, 0xA5, 0x2E, 0x0C, 0xC1, 0x37, 0x01, 0x00, 0x20, 0x0C, 0x00, 0x30, 0xA9,
            0x94, 0xEE, 0xED, 0xFF, 0x84, 0xE4, 0x00, 0xC0, 0xFD, 0x10, 0x91, 0x88, 0x98, 0x59, 0x44, 0x55,
            0xCD, 0xCC, 0xDD, 0x23, 0x32, 0xB3, 0xAA, 0xBA, 0x67, 0x66, 0xF7, 0x11, 0x04, 0x0F, 0x06, 0x00,
            0xC3, 0x00, 0x00, 0xC0, 0x3E, 0x9E, 0x6D, 0xDB, 0xB6, 0x8B, 0x33, 0x97, 0x90, 0x24, 0x45, 0x51,
            0x34, 0xCD, 0x30, 0x2C, 0xCB, 0x72, 0x1C, 0xCF, 0x0B, 0x82, 0x28, 0x4A, 0x92, 0x24, 0xCB, 0x8A,
            0xA2, 0xAA, 0x9A, 0xA6, 0xEB, 0x86, 0x61, 0x9A, 0x96, 0x65, 0xDB, 0x8E, 0xEB, 0x7A, 0x9E, 0xEF,
            0x07, 0x41, 0x18, 0x45, 0x71, 0x9C, 0x24, 0x69, 0x96, 0xE5, 0x79, 0x51, 0x96, 0x55, 0x55, 0x37,
            0x4D, 0xDB, 0x75, 0x7D, 0x3F, 0x8C, 0xE3, 0x34, 0xCF, 0xCB, 0xBA, 0x6E, 0xFB, 0x7E, 0x9C, 0xD7,
            0x75, 0x3F, 0xCF, 0xFB, 0x01, 0x00, 0x11, 0xC2, 0x7F, 0xC3, 0xE0, 0x00, 0x18, 0x47, 0x10, 0x85,
            0xE1, 0xAB, 0x6D, 0xDB, 0x6E, 0x63, 0x67, 0xE7, 0xFD, 0xB5, 0x6D, 0xDB, 0x36, 0x77, 0x6F, 0x6A,
            0xDB, 0xB6, 0x62, 0xDB, 0xB6, 0x6D, 0xDB, 0xB6, 0x3E, 0xE8, 0xE9, 0xEB, 0x1B, 0x18, 0x1A, 0x19,
            0x1B, 0x9B, 0x98, 0x9A, 0x99, 0x99, 0x5B, 0x58, 0x5A, 0x59, 0x5B, 0xDB, 0xD8, 0xDA, 0xD9, 0xDB,
            0x3B, 0x38, 0x3A, 0x39, 0xBB, 0xB8, 0xBA, 0xB9, 0xB9, 0x7B, 0x78, 0x7A, 0x79, 0xFB, 0xF8, 0xFA,
            0xF9, 0xF9, 0x07, 0x04, 0x06, 0x05, 0x87, 0x84, 0x86, 0x85, 0x47, 0x44, 0x46, 0x45, 0xC7, 0xC4,
            0xC6, 0xC5, 0x27, 0x24, 0x26, 0x25, 0xA7, 0xA4, 0xA6, 0xA5, 0x67, 0x64, 0x66, 0x65, 0xE7, 0xE4,
            0xE6, 0xE5, 0x17, 0x14, 0x15, 0x97, 0x94, 0x96, 0x95, 0x57, 0x54, 0x56, 0xD5, 0xD4, 0xD6, 0xD5,
            0x37, 0x34, 0x36, 0x95, 0x76, 0x54, 0xD7, 0xB1, 0xBD, 0xD5, 0x4E, 0x57, 0xCB, 0xEB, 0xB4, 0xEE,
            0xE1, 0x97, 0x0E, 0xBA, 0xDD, 0xFE, 0x77, 0x14, 0x64, 0x37, 0x95, 0x84, 0xE5, 0x53, 0xF7, 0x08,
            0x43, 0xF0, 0x48, 0x78, 0xA9, 0xFD, 0x5F, 0x70, 0xE2, 0x3E, 0x42, 0xDF, 0x7E, 0x79, 0x42, 0xD0,
            0xA5, 0xCE, 0xCC, 0xF3, 0xD9, 0x30, 0xB6, 0x3D, 0x62, 0x16, 0x9B, 0xB7, 0x70, 0x1E, 0x73, 0x2B,
            0xDC, 0xC2, 0xFE, 0x07, 0x9C, 0x61, 0xA3, 0xCF, 0xDF, 0x61, 0x03, 0x7E, 0xBF, 0x64, 0x96, 0x97,
            0xBF, 0xB3, 0xF1, 0x55, 0xC6, 0xEC, 0xEB, 0x48, 0x67, 0x76, 0xAD, 0x3C, 0x88, 0x8D, 0xBA, 0x9A,
            0xCC, 0x8A, 0xAC, 0x4B, 0xD8, 0x8C, 0x68, 0x19, 0xC5, 0x2B, 0x74, 0xA7, 0xE9, 0x39, 0x03, 0xC8,
            0x4C, 0x36, 0x9A, 0xCA, 0xC5, 0xA9, 0xB4, 0x61, 0x87, 0x2A, 0x8D, 0xFB, 0x45, 0x74, 0x61, 0xD5,
            0x52, 0xC2, 0xBE, 0x8D, 0xA4, 0x99, 0xB2, 0x97, 0xC2, 0x3C, 0x4E, 0x91, 0x56, 0x3F, 0x89, 0x7E,
            0xEE, 0xBB, 0x43, 0x36, 0x67, 0x1F, 0xD3, 0x1E, 0xAB, 0x57, 0x54, 0x32, 0xFD, 0x13, 0x9D, 0x0A,
            0xFF, 0x49, 0x06, 0xFA, 0xFA, 0x34, 0xDE, 0xDC, 0x9C, 0x3A, 0xE7, 0xD8, 0x93, 0xE6, 0x22, 0x0F,
            0x62, 0xA1, 0x01, 0xB4, 0x90, 0x47, 0xD2, 0xD4, 0x35, 0x49, 0xB4, 0x7D, 0x6E, 0x36, 0xB5, 0xAC,
            0x2C, 0xA1, 0x98, 0xB3, 0xB5, 0xA4, 0x17, 0x29, 0x83, 0xFF, 0xC5, 0x4E, 0xD0, 0x1D, 0xD5, 0x1D,
            0x8B, 0xBD, 0x7A, 0x63, 0xD2, 0xF1, 0x01, 0x28, 0xEF, 0x3D, 0x14, 0xEE, 0xFA, 0x23, 0xF1, 0x53,
            0x18, 0x87, 0x3F, 0x6E, 0x93, 0x90, 0xA3, 0x31, 0x1D, 0xEF, 0x3F, 0x28, 0xA0, 0x53, 0x8E, 0x0A,
            0xBE, 0x8E, 0xD4, 0x44, 0xB3, 0xA6, 0x2E, 0xCA, 0x35, 0x80, 0xEB, 0x43, 0xE6, 0x21, 0x3B, 0x66,
            0x21, 0x7E, 0x5F, 0x5C, 0x0A, 0xB1, 0x7C, 0x05, 0x4E, 0x2D, 0x5D, 0x03, 0x2F, 0x69, 0x3D, 0x3A,
            0x3C, 0xDE, 0x84, 0xE2, 0xF3, 0x5B, 0x71, 0x4F, 0x6B, 0x07, 0xBA, 0xC4, 0xEC, 0x42, 0xFC, 0xEA,
            0xBD, 0xF8, 0xF0, 0x77, 0x3F, 0xBA, 0xC6, 0x1D, 0x44, 0xEF, 0xBC, 0xC3, 0x30, 0x8B, 0x39, 0x8A,
            0x7E, 0x7F, 0x8F, 0x63, 0xEF, 0x86, 0x93, 0xC8, 0xC9, 0x3E, 0x85, 0x92, 0x6D, 0x67, 0x10, 0x66,
            0x75, 0x16, 0xD9, 0x55, 0xE7, 0xF0, 0x60, 0xD4, 0x05, 0x0C, 0x50, 0xB8, 0x88, 0x98, 0xA9, 0x97,
            0x30, 0xB6, 0xCF, 0x65, 0xA8, 0xA5, 0x5F, 0xC6, 0xF2, 0x5F, 0x57, 0xF0, 0x6E, 0xDD, 0x55, 0x68,
            0x57, 0x5E, 0xC5, 0xE2, 0xBB, 0xD7, 0x10, 0xDC, 0x5D, 0x84, 0xEF, 0x4D, 0x11, 0x93, 0x4B, 0x44,
            0x04, 0xAC, 0x96, 0xF0, 0xF4, 0xBF, 0x04, 0xCD, 0x1A, 0x09, 0x77, 0x99, 0x1C, 0xD3, 0x45, 0x39,
            0x02, 0x2D, 0xE4, 0xE8, 0x92, 0x25, 0xC7, 0xF1, 0xBE, 0x1C, 0xB7, 0x34, 0x38, 0xFC, 0x37, 0x71,
            0x58, 0x9F, 0x6F, 0xFB, 0x09, 0x87, 0xC2, 0x2F, 0x8E, 0x6B, 0x56, 0x1C, 0xD5, 0x9E, 0x1C, 0x45,
            0x61, 0x1C, 0x46, 0x89, 0x1C, 0x57, 0xB2, 0x38, 0x9E, 0x17, 0x72, 0x28, 0x94, 0x73, 0xDC, 0xAF,
            0xE6, 0x08, 0xAD, 0xE3, 0x50, 0x69, 0xE4, 0x88, 0x6F, 0xE2, 0x28, 0x6A, 0xE6, 0x78, 0xD6, 0xC2,
            0x91, 0xD2, 0x76, 0x63, 0xDB, 0xAD
        };
    }
}
