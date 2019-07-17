using System;
using PeterO;
using PeterO.Numbers;

namespace PeterO.Cbor {
  internal sealed class CBORNumber {
    internal enum Kind {
      Integer,
      IEEEBinary64,
      EInteger,
      EDecimal,
      EFloat,
      ERational,
    }
    private readonly Kind kind;
    private readonly object value;
    public CBORNumber(Kind kind, object value) {
      this.kind = kind;
      this.value = value;
    }

    private static ICBORNumber GetNumberInterface(Kind kind) {
      switch(kind) {
        case Kind.Integer:
          return CBORObject.GetNumberInterface(0);
        case Kind.EInteger:
          return CBORObject.GetNumberInterface(1);
        case Kind.IEEEBinary64:
          return CBORObject.GetNumberInterface(8);
        case Kind.EDecimal:
          return CBORObject.GetNumberInterface(10);
        case Kind.EFloat:
          return CBORObject.GetNumberInterface(11);
        case Kind.ERational:
          return CBORObject.GetNumberInterface(12);
        default: return null;
      }
    }

    public CBORObject ToCBORObject(){
      return CBORObject.FromObject(this.value);
    }

    public static CBORNumber FromCBORObject(CBORObject o) {
      // TODO: Include integers and floats
      if (o.HasOneTag(2) || o.HasOneTag(3)) {
        return CheckEInteger(o);
      }
      if (o.HasOneTag(4) || o.HasOneTag(5) || o.HasOneTag(264) ||
          o.HasOneTag(265)) {
        return ConvertToDecimalFrac(o, o.MostOuterTag.ToInt32Checked());
      }
      if (o.HasOneTag(30)) {
        return CheckRationalNumber(o);
      }
      return null;
    }

    private static CBORNumber DecimalFracToNumber(
      CBORObject o,
      int tagName) {
      if (o.Type != CBORType.Array) {
        throw new CBORException("Big fraction must be an array");
      }
      if (o.Count != 2) {
        throw new CBORException("Big fraction requires exactly 2 items");
      }
      if (!o[0].IsIntegral) {
        throw new CBORException("Exponent is not an integer");
      }
      if (!o[1].IsIntegral) {
        throw new CBORException("Mantissa is not an integer");
      }
      // TODO: Limit to integers and tag 2/3
      EInteger exponent = o[0].AsEInteger();
      if ((tagName==4 || tagName==5) &&
         exponent.GetSignedBitLengthAsEInteger().CompareTo(64) > 0) {
        throw new CBORException("Exponent is too big");
      }
      // TODO: Limit to integers and tag 2/3
      EInteger mantissa = o[1].AsEInteger();
      if (exponent.IsZero) {
        // Exponent is 0, so return mantissa instead
        return CBORObject.FromObject(mantissa);
      }
      return (tagName==4 || tagName==264) ?
        new CBORNumber(
          Kind.EDecimal,
          EDecimal.Create(mantissa, exponent)) :
        new CBORNumber(
          Kind.EFloat,
          EFloat.Create(mantissa, exponent));
    }

    private static CBORNumber BignumToNumber(CBORObject o) {
      if (o.Type != CBORType.ByteString) {
        throw new CBORException("Byte array expected");
      }
      bool negative = o.HasMostInnerTag(3);
      byte[] data = o.GetByteString();
      if (data.Length <= 7) {
        long x = 0;
        for (var i = 0; i < data.Length; ++i) {
          x <<= 8;
          x |= ((long)data[i]) & 0xff;
        }
        if (negative) {
          x = -x;
          --x;
        }
        return new CBORNumber(Kind.Integer, x);
      }
      int neededLength = data.Length;
      byte[] bytes;
      EInteger bi;
      var extended = false;
      if (((data[0] >> 7) & 1) != 0) {
        // Increase the needed length
        // if the highest bit is set, to
        // distinguish negative and positive
        // values
        ++neededLength;
        extended = true;
      }
      bytes = new byte[neededLength];
      for (var i = 0; i < data.Length; ++i) {
        bytes[i] = data[data.Length - 1 - i];
        if (negative) {
          bytes[i] = (byte)((~((int)bytes[i])) & 0xff);
        }
      }
      if (extended) {
        bytes[bytes.Length - 1] = negative ? (byte)0xff : (byte)0;
      }
      bi = EInteger.FromBytes(bytes, true);
      if (bi.CanFitInInt64()) {
        return new CBORNumber(Kind.Integer, bi.ToInt64Checked());
      } else {
        return new CBORNumber(Kind.EInteger, bi);
      }
    }

    private static CBORNumber CheckRationalNumber(CBORObject obj) {
      if (obj.Type != CBORType.Array) {
        throw new CBORException("Rational number must be an array");
      }
      if (obj.Count != 2) {
        throw new CBORException("Rational number requires exactly 2 items");
      }
      CBORObject first = obj[0];
      CBORObject second = obj[1];
      // TODO: Limit to integers and tag 2/3
      if (!first.IsIntegral) {
        throw new CBORException("Rational number requires integer numerator");
      }
      // TODO: Limit to integers and tag 2/3
      if (!second.IsIntegral) {
        throw new CBORException("Rational number requires integer denominator");
      }
      if (second.Sign <= 0) {
        throw new CBORException(
           "Rational number requires denominator greater than 0");
      }
      return new CBORNumber(Kind.ERational,
        ERational.Create(first.AsEInteger(), second.AsEInteger()));
    }

    public CBORNumber Add(CBORObject b) {
      // ArgumentAssert.NotNull(b);
      CBORNumber a = this;
      object objA = a.ThisItem;
      object objB = b.ThisItem;
      Kind typeA = a.Kind;
      Kind typeB = b.Kind;
      if (typeA == Kind.Integer && typeB == Kind.Integer) {
        var valueA = (long)objA;
        var valueB = (long)objB;
        if ((valueA < 0 && valueB < Int64.MinValue - valueA) ||
                (valueA > 0 && valueB > Int64.MaxValue - valueA)) {
         // would overflow, convert to EInteger
          return new CBORNumber(Kind.EInteger,((EInteger)valueA) +
          (EInteger)valueB);
        }
        return new CBORNumber(Kind.Integer,valueA + valueB);
      }
      if (typeA == Kind.ERational ||
             typeB == Kind.ERational) {
        ERational e1 =
        GetNumberInterface(typeA).AsExtendedRational(objA);
        ERational e2 =
        GetNumberInterface(typeB).AsExtendedRational(objB);
        return new CBORNumber(Kind.ERational,e1.Add(e2));
      }
      if (typeA == Kind.EDecimal ||
             typeB == Kind.EDecimal) {
        EDecimal e1 =
        GetNumberInterface(typeA).AsExtendedDecimal(objA);
        EDecimal e2 =
        GetNumberInterface(typeB).AsExtendedDecimal(objB);
        return new CBORNumber(Kind.EDecimal,e1.Add(e2));
      }
      if (typeA == Kind.EFloat || typeB == Kind.EFloat || 
             typeA == Kind.IEEEBinary64 || typeB == Kind.IEEEBinary64) {
        EFloat e1 =
        GetNumberInterface(typeA).AsExtendedFloat(objA);
        EFloat e2 = GetNumberInterface(typeB).AsExtendedFloat(objB);
        return new CBORNumber(Kind.EFloat,e1.Add(e2));
      } else {
        EInteger b1 = GetNumberInterface(typeA).AsEInteger(objA);
        EInteger b2 = GetNumberInterface(typeB).AsEInteger(objB);
        return new CBORNumber(Kind.EInteger,b1 + (EInteger)b2);
      }
    }

    public CBORNumber Subtract(CBORObject b) {
      // ArgumentAssert.NotNull(b);
      CBORNumber a = this;
      object objA = a.ThisItem;
      object objB = b.ThisItem;
      Kind typeA = a.Kind;
      Kind typeB = b.Kind;
      if (typeA == Kind.Integer && typeB == Kind.Integer) {
        var valueA = (long)objA;
        var valueB = (long)objB;
        if ((valueB < 0 && Int64.MaxValue + valueB < valueA) ||
                (valueB > 0 && Int64.MinValue + valueB > valueA)) {
         // would overflow, convert to EInteger
          return new CBORNumber(Kind.EInteger,((EInteger)valueA) -
          (EInteger)valueB);
        }
        return new CBORNumber(Kind.Integer,valueA - valueB);
      }
      if (typeA == Kind.ERational || typeB == Kind.ERational) {
        ERational e1 =
        GetNumberInterface(typeA).AsExtendedRational(objA);
        ERational e2 =
        GetNumberInterface(typeB).AsExtendedRational(objB);
        return new CBORNumber(Kind.ERational,e1.Subtract(e2));
      }
      if (typeA == Kind.EDecimal || typeB == Kind.EDecimal) {
        EDecimal e1 =
        GetNumberInterface(typeA).AsExtendedDecimal(objA);
        EDecimal e2 =
        GetNumberInterface(typeB).AsExtendedDecimal(objB);
        return new CBORNumber(Kind.EDecimal,e1.Subtract(e2));
      }
      if (typeA == Kind.EFloat || typeB == Kind.EFloat ||
               typeA == Kind.IEEEBinary64 || typeB == Kind.IEEEBinary64) {
        EFloat e1 =
        GetNumberInterface(typeA).AsExtendedFloat(objA);
        EFloat e2 = GetNumberInterface(typeB).AsExtendedFloat(objB);
        return new CBORNumber(Kind.EFloat,e1.Subtract(e2));
      } else {
        EInteger b1 = GetNumberInterface(typeA).AsEInteger(objA);
        EInteger b2 = GetNumberInterface(typeB).AsEInteger(objB);
        return new CBORNumber(Kind.EInteger,b1 - (EInteger)b2);
      }
    }

/*
    /// <summary>Compares two CBOR numbers. In this implementation, the two
    /// numbers' mathematical values are compared. Here, NaN (not-a-number)
    /// is considered greater than any number. This method is not
    /// consistent with the Equals method.</summary>
    /// <param name='other'>A value to compare with.</param>
    /// <returns>Less than 0, if this value is less than the other object;
    /// or 0, if both values are equal; or greater than 0, if this value is
    /// less than the other object or if the other object is
    /// null.</returns>
    /// <exception cref='ArgumentException'>An internal error
    /// occurred.</exception>
*/
    public int CompareTo(CBORNumber other) {
      if (other == null) {
        return 1;
      }
      if (this == other) {
        return 0;
      }
      var cmp = 0;
      Kind typeA = this.kind;
      Kind typeB = other.kind;
      object objA = this.value;
      object objB = other.value;
      if (typeA == typeB) {
        switch (typeA) {
          case Kind.Integer: {
              var a = (long)objA;
              var b = (long)objB;
              cmp = (a == b) ? 0 : ((a < b) ? -1 : 1);
              break;
            }
          case Kind.EInteger: {
              var bigintA = (EInteger)objA;
              var bigintB = (EInteger)objB;
              cmp = bigintA.CompareTo(bigintB);
              break;
            }
          case Kind.IEEEBinary64: {
              var a = (double)objA;
              var b = (double)objB;
              // Treat NaN as greater than all other numbers
              cmp = Double.IsNaN(a) ? (Double.IsNaN(b) ? 0 : 1) :
                (Double.IsNaN(b) ? (-1) : ((a == b) ? 0 : ((a < b) ? -1 :
                    1)));
              break;
            }
          case Kind.EDecimal: {
              cmp = ((EDecimal)objA).CompareTo((EDecimal)objB);
              break;
            }
          case Kind.EFloat: {
              cmp = ((EFloat)objA).CompareTo(
                (EFloat)objB);
              break;
            }
          case Kind.ERational: {
              cmp = ((ERational)objA).CompareTo(
                (ERational)objB);
              break;
            }
          default: throw new ArgumentException("Unexpected data type");
        }
      } else {
        int s1 = GetNumberInterface(typeA).Sign(objA);
        int s2 = GetNumberInterface(typeB).Sign(objB);
        if (s1 != s2 && s1 != 2 && s2 != 2) {
          // if both types are numbers
          // and their signs are different
          return (s1 < s2) ? -1 : 1;
        }
        if (s1 == 2 && s2 == 2) {
          // both are NaN
          cmp = 0;
        } else if (s1 == 2) {
          // first object is NaN
          return 1;
        } else if (s2 == 2) {
          // second object is NaN
          return -1;
        } else {
          // DebugUtility.Log("a=" + this + " b=" + other);
          if (typeA == Kind.ERational) {
            ERational e1 =
GetNumberInterface(typeA).AsExtendedRational(objA);
            if (typeB == Kind.EDecimal) {
              EDecimal e2 =
GetNumberInterface(typeB).AsExtendedDecimal(objB);
              cmp = e1.CompareToDecimal(e2);
            } else {
              EFloat e2 = GetNumberInterface(typeB).AsExtendedFloat(objB);
              cmp = e1.CompareToBinary(e2);
            }
          } else if (typeB == Kind.ERational) {
            ERational e2 =
              GetNumberInterface(typeB).AsExtendedRational(objB);
            if (typeA == Kind.EDecimal) {
              EDecimal e1 =
              GetNumberInterface(typeA).AsExtendedDecimal(objA);
              cmp = e2.CompareToDecimal(e1);
              cmp = -cmp;
            } else {
              EFloat e1 =
              GetNumberInterface(typeA).AsExtendedFloat(objA);
              cmp = e2.CompareToBinary(e1);
              cmp = -cmp;
            }
          } else if (typeA == Kind.EDecimal ||
                    typeB == Kind.EDecimal) {
            EDecimal e1 = null;
            EDecimal e2 = null;
            if (typeA == Kind.EFloat) {
              var ef1 = (EFloat)objA;
              e2 = (EDecimal)objB;
              cmp = e2.CompareToBinary(ef1);
              cmp = -cmp;
            } else if (typeB == Kind.EFloat) {
              var ef1 = (EFloat)objB;
              e2 = (EDecimal)objA;
              cmp = e2.CompareToBinary(ef1);
            } else {
              e1 = GetNumberInterface(typeA).AsExtendedDecimal(objA);
              e2 = GetNumberInterface(typeB).AsExtendedDecimal(objB);
              cmp = e1.CompareTo(e2);
            }
          } else if (typeA == Kind.EFloat || typeB ==
                Kind.EFloat || typeA == Kind.IEEEBinary64 || typeB ==
                Kind.IEEEBinary64) {
            EFloat e1 =
            GetNumberInterface(typeA).AsExtendedFloat(objA);
            EFloat e2 = GetNumberInterface(typeB).AsExtendedFloat(objB);
            cmp = e1.CompareTo(e2);
          } else {
            EInteger b1 = GetNumberInterface(typeA).AsEInteger(objA);
            EInteger b2 = GetNumberInterface(typeB).AsEInteger(objB);
            cmp = b1.CompareTo(b2);
          }
        }
      }
      return cmp;
    }
  }
}