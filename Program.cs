using System;
using System.Collections.Generic;
using System.Linq;
using Functional.Maybe;
using static MyExtentions;

class Program {
    static void Main (string[] args) {
        var bingoNumMin = 1;
        var bingoNumCount = 100;
        var cardSize = 5;

        var cardModel = Enumerable.Range (bingoNumMin, bingoNumCount)
            .Apply (Shuffle)
            .Buffer (cardSize)
            .Apply (MakeMiddleHole);

        var cardView = cardModel.Select (row =>
                row.Select (num =>
                    num.Select (x => $"{x:D3}|")
                    .OrElse ("   |") //空いているものは空白3文字
                ).Apply (strs => String.Concat (strs)) + "\n"
            ).Apply (strs => String.Concat (strs))
            .TrimEnd ('\n');

        Console.WriteLine (cardView);

        // ローカル関数定義

        // シャッフルして、cardSize^2に切り取る
        IEnumerable<T> Shuffle<T> (IEnumerable<T> nums) {
            var rndGen = new Random ();
            return Enumerable.Range (0, cardSize * cardSize)
                .Aggregate (
                    (rest: nums.ToList (), shuffled: Enumerable.Empty<T> ()),
                    (arg, _) => {
                        var (rest, shuffled) = arg;
                        var rndIndex = rndGen.Next (rest.Count);
                        var nextShuffled = shuffled.Append (rest.ElementAt (rndIndex));
                        rest.RemoveAt (rndIndex);
                        return (rest, nextShuffled);
                    }
                ).shuffled;
        }

        // IEnumerable<T> ShuffleRec<T>(Random rndGen, IEnumerable<T> rest, IEnumerable<T> shuffled, int i){
        //     if (i == 0){
        //         return shuffled;
        //     } else {
        //         var rndIndex = rndGen.Next(rest.Count());
        //     }
        // }

        // Bingoは真ん中だけ空いているので、真ん中だけNothingにする
        IEnumerable<IEnumerable<Maybe<int>>> MakeMiddleHole (IEnumerable<IEnumerable<int>> nums2D) {
            return nums2D.Select ((row, i) =>
                row.Select ((num, j) => {
                    var middle = cardSize / 2;
                    if (i == middle && j == middle) {
                        return Maybe<int>.Nothing;
                    } else {
                        return num.ToMaybe ();
                    }
                }));
        }
    }
}
static class MyExtentions {
    /// <summary>
    /// 与えられた関数に適用する（メソッドチェーンを使って書きたいときに使う）
    /// </summary>
    public static TR Apply<T, TR> (this T arg, Func<T, TR> func) {
        return func (arg);
    }
}