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

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace TWCore
{
    /// <summary>
    /// Beep Engine
    /// </summary>
    public static class BeepEngine
    {
        #region Fields
        const int TempoMs = 60000;
        static readonly Dictionary<string, int[]> Notes;
        static readonly int[] Tempos;
        static readonly int[] TemposDot;
        static readonly int[] TemposTriplets;
        #endregion

        #region .ctor
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static BeepEngine()
        {
            Notes = new Dictionary<string, int[]>
            {
                ["C"] = new[] { 65, 131, 262, 523, 1047 },
                ["C#"] = new[] { 69, 139, 277, 554, 1109 },
                ["D"] = new[] { 73, 147, 294, 587, 1175 },
                ["D#"] = new[] { 78, 156, 311, 622, 1245 },
                ["E"] = new[] { 82, 165, 330, 659, 1319 },
                ["F"] = new[] { 87, 175, 349, 698, 1397 },
                ["F#"] = new[] { 93, 185, 370, 740, 1480 },
                ["G"] = new[] { 98, 196, 392, 784, 1568 },
                ["G#"] = new[] { 104, 208, 415, 831, 1661 },
                ["A"] = new[] { 110, 220, 440, 880, 1760 },
                ["A#"] = new[] { 117, 233, 466, 932, 1865 },
                ["B"] = new[] { 123, 247, 494, 988, 1976 }
            };
            Notes["Db"] = Notes["C#"];
            Notes["Eb"] = Notes["D#"];
            Notes["Gb"] = Notes["F#"];
            Notes["Ab"] = Notes["G#"];
            Notes["Bb"] = Notes["A#"];

            Tempos = new[]
            {
                TempoMs * 4,
                TempoMs * 2,
                TempoMs,
                TempoMs / 2,
                TempoMs / 4,
                TempoMs / 8,
                TempoMs / 16
            };
            TemposDot = new[]
            {
                Tempos[0] + Tempos[1],
                Tempos[1] + Tempos[2],
                Tempos[2] + Tempos[3],
                Tempos[3] + Tempos[4],
                Tempos[4] + Tempos[5],
                Tempos[5] + Tempos[6]
            };
            TemposTriplets = new[]
            {
                Tempos[0]/3,
                Tempos[1]/3,
                Tempos[2]/3,
                Tempos[3]/3,
                Tempos[4]/3,
                Tempos[5]/3,
                Tempos[6]/3
            };
        }
        #endregion

        #region Private 
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Nullable<(int, int)> GetFreqAndTempo(string term, int tempo = 120, int octave = 4)
        {
            if (string.IsNullOrEmpty(term)) return null;
            var termArr = term.SplitAndTrim('-');

            var useTriplets = false;
            var useDot = false;
            var note = string.Empty;
            var dIndex = 0;
            var dOctave = octave - 2;

            if (termArr.Length < 2) return null;
            if (termArr[0][0] == 'T')
                useTriplets = true;
            if (termArr[0][0] == 'D')
                useDot = true;
            int duration;
            if (useTriplets || useDot)
            {
                if (!int.TryParse(termArr[0].Substring(1), out duration))
                    duration = 1;
            }
            else
            {
                if (!int.TryParse(termArr[0], out duration))
                    duration = 1;
            }
            switch (duration)
            {
                case 1: dIndex = 0; break;
                case 2: dIndex = 1; break;
                case 4: dIndex = 2; break;
                case 8: dIndex = 3; break;
                case 16: dIndex = 4; break;
                case 32: dIndex = 5; break;
                case 64: dIndex = 6; break;
            }
            note = termArr[1];
            if (termArr.Length > 2 && int.TryParse(termArr[2], out var deltaOctave))
                dOctave += deltaOctave;

            if (useTriplets)
                return (Notes[note][dOctave], TemposTriplets[dIndex] / tempo);
            if (useDot)
                return (Notes[note][dOctave], TemposDot[dIndex] / tempo);
            return (Notes[note][dOctave], Tempos[dIndex] / tempo);
        }
        #endregion

        #region Public
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<(int, int)> GetFreqTempoPairs(string terms, int tempo = 120, int octave = 4)
        {
            if (string.IsNullOrEmpty(terms)) return null;
            var termArray = terms.SplitAndTrim(',');
            var lstTerms = new List<(int, int)>();
            foreach (var term in termArray)
            {
                var resp = GetFreqAndTempo(term, tempo, octave);
                if (resp != null)
                    lstTerms.Add(resp.Value);
            }
            return lstTerms;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Play(string terms, int tempo = 120, int octave = 4)
        {
            var lstNotes = GetFreqTempoPairs(terms, tempo, octave);
            foreach (var note in lstNotes)
                Console.Beep(note.Item1, note.Item2);
        }
        #endregion
    }
}
