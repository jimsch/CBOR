/*
Written in 2013 by Peter O.

Parts of the code were adapted by Peter O. from
the public-domain code from the library
CryptoPP by Wei Dai.

Any copyright is dedicated to the Public Domain.
http://creativecommons.org/publicdomain/zero/1.0/
If you like this, you should donate to Peter O.
at: http://upokecenter.dreamhosters.com/articles/donate-now-2/
 */
using System;
using PeterO.Numbers;

namespace PeterO {
    /// <include file='docs.xml' 
    /// path='docs/doc[@name="T:PeterO.BigInteger"]'/>
  public sealed partial class BigInteger : IComparable<BigInteger>,
    IEquatable<BigInteger> {
    /// <include file='docs.xml' 
    /// path='docs/doc[@name="F:PeterO.BigInteger.ONE"]'/>
#if CODE_ANALYSIS
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Security",
      "CA2104",
      Justification = "BigInteger is immutable")]
#endif

    public static readonly BigInteger ONE = new BigInteger(EInteger.One);

    internal readonly EInteger ei;

    internal BigInteger(EInteger ei) {
      if ((ei) == null) {
  throw new ArgumentNullException("ei");
}
      this.ei = ei;
    }

    /// <include file='docs.xml' 
    /// path='docs/doc[@name="F:PeterO.BigInteger.TEN"]'/>
#if CODE_ANALYSIS
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Security",
      "CA2104",
      Justification = "BigInteger is immutable")]
#endif

    public static readonly BigInteger TEN = BigInteger.valueOf(10);

    /// <include file='docs.xml' 
    /// path='docs/doc[@name="F:PeterO.BigInteger.ZERO"]'/>
#if CODE_ANALYSIS
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Security",
      "CA2104",
      Justification = "BigInteger is immutable")]
#endif
    public static readonly BigInteger ZERO = new BigInteger(EInteger.Zero);

    /// <include file='docs.xml' 
    /// path='docs/doc[@name="P:PeterO.BigInteger.IsEven"]'/>
    public bool IsEven { get {
 return this.ei.IsEven;
} }

    /// <include file='docs.xml' 
    /// path='docs/doc[@name="P:PeterO.BigInteger.IsZero"]'/>
    public bool IsZero { get {
 return this.ei.IsZero;
} }

    /// <include file='docs.xml' 
    /// path='docs/doc[@name="P:PeterO.BigInteger.Sign"]'/>
    public int Sign { get {
 return this.ei.Sign;
} }

    /// <include file='docs.xml' 
    /// path='docs/doc[@name="M:PeterO.BigInteger.fromByteArray(System.Byte[],System.Boolean)"]'/>
    [Obsolete("Renamed to 'fromBytes'.")]
    public static BigInteger fromByteArray(byte[] bytes, bool littleEndian) {
      return new BigInteger(EInteger.FromBytes(bytes, littleEndian));
 }

    /// <include file='docs.xml' 
    /// path='docs/doc[@name="M:PeterO.BigInteger.fromBytes(System.Byte[],System.Boolean)"]'/>
    public static BigInteger fromBytes(byte[] bytes, bool littleEndian) {
      return new BigInteger(EInteger.FromBytes(bytes, littleEndian));
    }

    /// <include file='docs.xml' 
    /// path='docs/doc[@name="M:PeterO.BigInteger.fromRadixString(System.String,System.Int32)"]'/>
    public static BigInteger fromRadixString(string str, int radix) {
      return new BigInteger(EInteger.FromRadixString(str, radix));
    }

    /// <include file='docs.xml' 
    /// path='docs/doc[@name="M:PeterO.BigInteger.fromRadixSubstring(System.String,System.Int32,System.Int32,System.Int32)"]'/>
    public static BigInteger fromRadixSubstring(
      string str,
      int radix,
      int index,
      int endIndex) {
 return new BigInteger(EInteger.FromRadixSubstring(str,
        radix, index, endIndex));
    }

    /// <include file='docs.xml' 
    /// path='docs/doc[@name="M:PeterO.BigInteger.fromString(System.String)"]'/>
    public static BigInteger fromString(string str) {
return new BigInteger(EInteger.fromString(str));
}

    /// <include file='docs.xml' 
    /// path='docs/doc[@name="M:PeterO.BigInteger.fromSubstring(System.String,System.Int32,System.Int32)"]'/>
    public static BigInteger fromSubstring(string str,
      int index,
      int endIndex) {
return new BigInteger(EInteger.FromSubstring(str, index, endIndex));
}

    /// <include file='docs.xml' 
    /// path='docs/doc[@name="M:PeterO.BigInteger.valueOf(System.Int64)"]'/>
    public static BigInteger valueOf(long longerValue) {
      return new BigInteger(EInteger.FromInt64(longerValue));
 }

    /// <include file='docs.xml' 
    /// path='docs/doc[@name="M:PeterO.BigInteger.abs"]'/>
    public BigInteger abs() {
      return new BigInteger(this.ei.Abs());
 }

    /// <include file='docs.xml' 
    /// path='docs/doc[@name="M:PeterO.BigInteger.add(PeterO.BigInteger)"]'/>
    public BigInteger add(BigInteger bigintAugend) {
      if ((bigintAugend) == null) {
  throw new ArgumentNullException("bigintAugend");
}
return new BigInteger(this.ei.add(bigintAugend.ei));
 }

    /// <include file='docs.xml' 
    /// path='docs/doc[@name="M:PeterO.BigInteger.bitLength"]'/>
    public int bitLength() {
return this.ei.bitLength();
 }

    /// <include file='docs.xml' 
    /// path='docs/doc[@name="M:PeterO.BigInteger.canFitInInt"]'/>
    public bool canFitInInt() {
return this.ei.canFitInInt();
      }

    /// <include file='docs.xml' 
    /// path='docs/doc[@name="M:PeterO.BigInteger.CompareTo(PeterO.BigInteger)"]'/>
    public int CompareTo(BigInteger other) {
      return (other == null) ? (1) : (this.ei.CompareTo(other.ei));
 }

    /// <include file='docs.xml' 
    /// path='docs/doc[@name="M:PeterO.BigInteger.divide(PeterO.BigInteger)"]'/>
    public BigInteger divide(BigInteger bigintDivisor) {
      if ((bigintDivisor) == null) {
        throw new ArgumentNullException("bigintDivisor");
      }
      return new BigInteger(this.ei.divide(bigintDivisor.ei));
 }

    /// <include file='docs.xml' 
    /// path='docs/doc[@name="M:PeterO.BigInteger.divideAndRemainder(PeterO.BigInteger)"]'/>
    public BigInteger[] divideAndRemainder(BigInteger divisor) {
      if ((divisor) == null) {
  throw new ArgumentNullException("divisor");
}
      EInteger[] eia = this.ei.DivRem(divisor.ei);
    return new BigInteger[] { new BigInteger(eia[0]), new BigInteger(eia[1])
        };
 }

    /// <include file='docs.xml' 
    /// path='docs/doc[@name="M:PeterO.BigInteger.Equals(System.Object)"]'/>
    public override bool Equals(object obj) {
      var bi = obj as BigInteger;
      return (bi == null) ? (false) : (this.ei.Equals(bi.ei));
}

    /// <include file='docs.xml' 
    /// path='docs/doc[@name="M:PeterO.BigInteger.gcd(PeterO.BigInteger)"]'/>
    public BigInteger gcd(BigInteger bigintSecond) {
  if ((bigintSecond) == null) {
  throw new ArgumentNullException("bigintSecond");
}
return new BigInteger(this.ei.gcd(bigintSecond.ei));
}

    /// <include file='docs.xml' 
    /// path='docs/doc[@name="M:PeterO.BigInteger.getDigitCount"]'/>
    public int getDigitCount() {
      return this.ei.getDigitCount();
 }

    /// <include file='docs.xml' 
    /// path='docs/doc[@name="M:PeterO.BigInteger.GetHashCode"]'/>
    public override int GetHashCode() {
      return this.ei.GetHashCode();
 }

    /// <include file='docs.xml' 
    /// path='docs/doc[@name="M:PeterO.BigInteger.getLowBit"]'/>
    public int getLowBit() {
      return this.ei.getLowBit();
 }

    /// <include file='docs.xml' 
    /// path='docs/doc[@name="M:PeterO.BigInteger.getLowestSetBit"]'/>
    [Obsolete("Renamed to getLowBit.")]
    public int getLowestSetBit() {
      return getLowBit();
 }

    /// <include file='docs.xml' 
    /// path='docs/doc[@name="M:PeterO.BigInteger.getUnsignedBitLength"]'/>
    public int getUnsignedBitLength() {
      return getUnsignedBitLength();
 }

    /// <include file='docs.xml' 
    /// path='docs/doc[@name="M:PeterO.BigInteger.intValue"]'/>
    [Obsolete(
  "To make the conversion intention clearer use the 'intValueChecked' and 'intValueUnchecked' methods instead. Replace 'intValue' with 'intValueChecked' in your code." )]
    public int intValue() {
return this.ei.AsInt32Checked();
}

    /// <include file='docs.xml' 
    /// path='docs/doc[@name="M:PeterO.BigInteger.intValueChecked"]'/>
    public int intValueChecked() {
return this.ei.AsInt32Checked();
}

    /// <include file='docs.xml' 
    /// path='docs/doc[@name="M:PeterO.BigInteger.intValueUnchecked"]'/>
    public int intValueUnchecked() {
return this.ei.AsInt32Unchecked();
}

    /// <include file='docs.xml' 
    /// path='docs/doc[@name="M:PeterO.BigInteger.longValue"]'/>
    [Obsolete(
  "To make the conversion intention clearer use the 'longValueChecked' and 'longValueUnchecked' methods instead. Replace 'longValue' with 'longValueChecked' in your code." )]
    public long longValue() {
return this.ei.longValueChecked();
}

    /// <include file='docs.xml' 
    /// path='docs/doc[@name="M:PeterO.BigInteger.longValueChecked"]'/>
    public long longValueChecked() {
return this.ei.longValueChecked();
}

    /// <include file='docs.xml' 
    /// path='docs/doc[@name="M:PeterO.BigInteger.longValueUnchecked"]'/>
    public long longValueUnchecked() {
      return this.ei.longValueUnchecked();
 }

    /// <include file='docs.xml' 
    /// path='docs/doc[@name="M:PeterO.BigInteger.mod(PeterO.BigInteger)"]'/>
    public BigInteger mod(BigInteger divisor) {
  if ((divisor) == null) {
  throw new ArgumentNullException("divisor");
}
return new BigInteger(this.ei.mod(divisor.ei));
}

    /// <include file='docs.xml' 
    /// path='docs/doc[@name="M:PeterO.BigInteger.ModPow(PeterO.BigInteger,PeterO.BigInteger)"]'/>
    public BigInteger ModPow(BigInteger pow, BigInteger mod) {
  if ((pow) == null) {
  throw new ArgumentNullException("pow");
}
  if ((mod) == null) {
  throw new ArgumentNullException("mod");
}
return new BigInteger(this.ei.ModPow(pow.ei, mod.ei));
}

    /// <include file='docs.xml' 
    /// path='docs/doc[@name="M:PeterO.BigInteger.multiply(PeterO.BigInteger)"]'/>
    public BigInteger multiply(BigInteger bigintMult) {
      if ((bigintMult) == null) {
        throw new ArgumentNullException("bigintMult");
      }
      return new BigInteger(this.ei.multiply(bigintMult.ei));
    }

    /// <include file='docs.xml' 
    /// path='docs/doc[@name="M:PeterO.BigInteger.negate"]'/>
    public BigInteger negate() {
      return new BigInteger(this.ei.negate());
 }

    /// <include file='docs.xml' 
    /// path='docs/doc[@name="M:PeterO.BigInteger.pow(System.Int32)"]'/>
    public BigInteger pow(int powerSmall) {
return new BigInteger(this.ei.pow(powerSmall));
}

    /// <include file='docs.xml' 
    /// path='docs/doc[@name="M:PeterO.BigInteger.PowBigIntVar(PeterO.BigInteger)"]'/>
    public BigInteger PowBigIntVar(BigInteger power) {
  if ((power) == null) {
  throw new ArgumentNullException("power");
}
return new BigInteger(this.ei.PowBigIntVar(power.ei));
}

    /// <include file='docs.xml' 
    /// path='docs/doc[@name="M:PeterO.BigInteger.remainder(PeterO.BigInteger)"]'/>
    public BigInteger remainder(BigInteger divisor) {
  if ((divisor) == null) {
  throw new ArgumentNullException("divisor");
}
return new BigInteger(this.ei.remainder(divisor.ei));
}

    /// <include file='docs.xml' 
    /// path='docs/doc[@name="M:PeterO.BigInteger.shiftLeft(System.Int32)"]'/>
    public BigInteger shiftLeft(int numberBits) {
      return new BigInteger(this.ei.ShiftLeft(numberBits));
 }

    /// <include file='docs.xml' 
    /// path='docs/doc[@name="M:PeterO.BigInteger.shiftRight(System.Int32)"]'/>
    public BigInteger shiftRight(int numberBits) {
      return new BigInteger(this.ei.ShiftRight(numberBits));
    }

    /// <include file='docs.xml' 
    /// path='docs/doc[@name="M:PeterO.BigInteger.sqrt"]'/>
    public BigInteger sqrt() {
      return new BigInteger(this.ei.Sqrt());
    }

    /// <include file='docs.xml' 
    /// path='docs/doc[@name="M:PeterO.BigInteger.sqrtWithRemainder"]'/>
    public BigInteger[] sqrtWithRemainder() {
      EInteger[] eia = this.ei.SqrtRem();
      return new BigInteger[] { new BigInteger(eia[0]), new BigInteger(eia[1])
        };
    }

    /// <include file='docs.xml' 
    /// path='docs/doc[@name="M:PeterO.BigInteger.subtract(PeterO.BigInteger)"]'/>
    public BigInteger subtract(BigInteger subtrahend) {
      if ((subtrahend) == null) {
  throw new ArgumentNullException("subtrahend");
}
      return new BigInteger(this.ei.Subtract(subtrahend.ei));
 }

    /// <include file='docs.xml' 
    /// path='docs/doc[@name="M:PeterO.BigInteger.testBit(System.Int32)"]'/>
    public bool testBit(int index) {
return this.ei.testBit(index);
}

    /// <include file='docs.xml' 
    /// path='docs/doc[@name="M:PeterO.BigInteger.toByteArray(System.Boolean)"]'/>
    [Obsolete("Renamed to 'toBytes'.")]
    public byte[] toByteArray(bool littleEndian) {
      return toBytes(littleEndian);
 }

    /// <include file='docs.xml' 
    /// path='docs/doc[@name="M:PeterO.BigInteger.toBytes(System.Boolean)"]'/>
    public byte[] toBytes(bool littleEndian) {
      return this.ei.toBytes(littleEndian);
 }

    /// <include file='docs.xml' 
    /// path='docs/doc[@name="M:PeterO.BigInteger.toRadixString(System.Int32)"]'/>
    public string toRadixString(int radix) {
      return this.ei.toRadixString(radix);
 }

    /// <include file='docs.xml' 
    /// path='docs/doc[@name="M:PeterO.BigInteger.ToString"]'/>
    public override string ToString() {
      return this.ei.ToString();
    }
  }
}
