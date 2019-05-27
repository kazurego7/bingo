using System;
using System.Collections.Generic;
using System.Linq;
using Functional.Maybe;
using static MyExtentions;

class Program {
    static void Main (string[] args) {
        var bingoNumMin = 1;
        var bingoNumCount = 10000;
        var cardSize = 33;

        var cardModel = Enumerable.Range (bingoNumMin, bingoNumCount)
            .Apply (nums => ShuffleTake (nums, cardSize * cardSize, new Random ()))
            .Buffer (cardSize)
            .Apply (MakeMiddleHole);

        var cardView = cardModel.Select (row =>
                row.Select (num =>
                    num.Select (x => $"{x:D4}|")
                    .OrElse ("    |") //空いているものは空白4文字
                ).Apply (strs => String.Concat (strs)) + "\n"
            ).Apply (strs => String.Concat (strs))
            .TrimEnd ('\n');

        Console.WriteLine (cardView);

        // ローカル関数定義

        // numsをシャッフルし、countだけ先頭からとる
        IEnumerable<T> ShuffleTake<T> (IEnumerable<T> nums, int count, Random rndGen) {
            return Enumerable.Range (0, count)
                .Aggregate (
                    (rest: nums, shuffled: Enumerable.Empty<T> ()),
                    (arg, _) => {
                        var (rest, shuffled) = arg;
                        var rndIndex = rndGen.Next (rest.Count ());
                        var nextRest = rest.RemoveAt (rndIndex);
                        var nextShuffled = shuffled.Append (rest.ElementAt (rndIndex));
                        return (nextRest, nextShuffled);
                    }
                ).shuffled;
        }

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

    /// <summary>
    /// インデックスの要素を取り除く
    /// </summary>
    public static IEnumerable<T> RemoveAt<T> (this IEnumerable<T> source, int index) {
        return source.Where ((x, i) => i != index);
    }
}