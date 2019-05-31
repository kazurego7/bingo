using System;
using System.Collections.Generic;
using System.Linq;
using Functional.Maybe;

namespace Bingo {
    class Alias<RealName> {
        public RealName Real { get; }
        public Alias (RealName real) => Real = real;
    }
    class Program {

        sealed class SquareNum : Alias<Maybe<int>> {
            public SquareNum (Maybe<int> real) : base (real) { }
        }

        sealed class CardModel : Alias<IEnumerable<IEnumerable<SquareNum>>> {
            public CardModel (IEnumerable<IEnumerable<SquareNum>> real) : base (real) { }
        }

        static void Main (string[] args) {
            var bingoNumMin = 1;
            var bingoNumCount = 100000;
            var cardSize = 100;

            var cardModel = Enumerable.Range (bingoNumMin, bingoNumCount)
                .Apply (nums => Shuffle2 (nums, new Random ()))
                .Take (cardSize * cardSize)
                .Buffer (cardSize)
                .Apply (MakeMiddleHole);

            var cardView = cardModel.Real
                .Select (row =>
                    row.Select (bingoSquare =>
                        bingoSquare.Real
                        .Select (x => $"{x:D4}|")
                        .OrElse ("    |") //空いているものは空白4文字
                    ).Apply (strs => String.Concat (strs)) + "\n"
                ).Apply (strs => String.Concat (strs))
                .TrimEnd ('\n');

            Console.WriteLine (cardView);

            // ローカル関数定義

            // numsをシャッフルする(破壊的代入なし)
            // O(N^2)
            IEnumerable<T> Shuffle<T> (IEnumerable<T> nums, Random rndGen) {
                return nums
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

            // numsをシャッフルする(破壊的代入あり)
            // O(N^2)だが、遅延性があり後でTakeしてるのでO(K^2)
            IEnumerable<T> Shuffle2<T> (IEnumerable<T> nums, Random rndGen) {
                var rest = nums.ToList ();
                return nums
                    .Aggregate (
                        Enumerable.Empty<T> (),
                        (shuffled, _) => {
                            var rndIndex = rndGen.Next (rest.Count);
                            var nextShuffled = shuffled.Append (rest[rndIndex]);
                            rest.RemoveAt (rndIndex);
                            return nextShuffled;
                        }
                    );
            }

            // Bingoは真ん中だけ空いているので、真ん中だけNothingにする
            CardModel MakeMiddleHole (IEnumerable<IEnumerable<int>> nums2D) {
                return nums2D.Select ((row, i) =>
                    row.Select ((num, j) => {
                        var middle = cardSize / 2;
                        if (i == middle && j == middle) {
                            return new SquareNum (Maybe<int>.Nothing);
                        } else {
                            return new SquareNum (num.ToMaybe ());
                        }
                    })
                ).Apply (bingoSquare2DArray => new CardModel (bingoSquare2DArray));
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
}