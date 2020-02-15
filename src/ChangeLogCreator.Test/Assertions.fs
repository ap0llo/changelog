module Assertions

open Xunit

let mustBeEqualTo (expected:'a) (actual:'a) = Assert.Equal<'a>(expected, actual)


