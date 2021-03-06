using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using PeterO;
using PeterO.Cbor;
using PeterO.Numbers;

namespace Test {
  internal static class CBORTestCommon {
    internal static readonly EDecimal DecPosInf =
      EDecimal.PositiveInfinity;

    internal static readonly EDecimal DecNegInf =
      EDecimal.NegativeInfinity;

    internal static readonly EFloat FloatPosInf =
      EFloat.PositiveInfinity;

    internal static readonly EFloat FloatNegInf =
      EFloat.NegativeInfinity;

    internal static readonly ERational RatPosInf =
      ERational.PositiveInfinity;

    internal static readonly ERational RatNegInf =
      ERational.NegativeInfinity;

    public static CBORObject RandomNumber(RandomGenerator rand) {
      object o = null;
      switch (rand.UniformInt(6)) {
        case 0:
          o = RandomObjects.RandomDouble(
            rand,
            Int32.MaxValue);
          return ToObjectTest.TestToFromObjectRoundTrip(o);
        case 1:
          o = RandomObjects.RandomSingle(
            rand,
            Int32.MaxValue);
          return ToObjectTest.TestToFromObjectRoundTrip(o);
        case 2:
          return ToObjectTest.TestToFromObjectRoundTrip (
              RandomObjects.RandomEInteger(rand));
        case 3:
          return ToObjectTest.TestToFromObjectRoundTrip (
              RandomObjects.RandomEFloat(rand));
        case 4:
          o = RandomObjects.RandomEDecimal(rand);
          return ToObjectTest.TestToFromObjectRoundTrip(o);
        case 5:
          o = RandomObjects.RandomInt64(rand);
          return ToObjectTest.TestToFromObjectRoundTrip(o);
        default: throw new InvalidOperationException();
      }
    }

    public static CBORObject RandomNumberOrRational(RandomGenerator rand) {
      object o = null;
      switch (rand.UniformInt(7)) {
        case 0:
          o = RandomObjects.RandomDouble(
            rand,
            Int32.MaxValue);
          return ToObjectTest.TestToFromObjectRoundTrip(o);
        case 1:
          o = RandomObjects.RandomSingle(
            rand,
            Int32.MaxValue);
          return ToObjectTest.TestToFromObjectRoundTrip(o);
        case 2:
          return ToObjectTest.TestToFromObjectRoundTrip (
              RandomObjects.RandomEInteger(rand));
        case 3:
          return ToObjectTest.TestToFromObjectRoundTrip (
              RandomObjects.RandomEFloat(rand));
        case 4:
          o = RandomObjects.RandomEDecimal(rand);
          return ToObjectTest.TestToFromObjectRoundTrip(o);
        case 5:
          o = RandomObjects.RandomInt64(rand);
          return ToObjectTest.TestToFromObjectRoundTrip(o);
        case 6:
          o = RandomObjects.RandomERational(rand);
          return ToObjectTest.TestToFromObjectRoundTrip(o);
        default: throw new InvalidOperationException();
      }
    }

    public static CBORObject RandomCBORMap(RandomGenerator rand, int depth) {
      int x = rand.UniformInt(100);
      int count = (x < 80) ? 2 : ((x < 93) ? 1 : ((x < 98) ? 0 : 10));
      CBORObject cborRet = CBORObject.NewMap();
      for (var i = 0; i < count; ++i) {
        CBORObject key = RandomCBORObject(rand, depth + 1);
        CBORObject value = RandomCBORObject(rand, depth + 1);
        cborRet[key] = value;
      }
      return cborRet;
    }

    public static EInteger RandomEIntegerMajorType0(RandomGenerator rand) {
      int v = rand.UniformInt(0x10000);
      EInteger ei = EInteger.FromInt32(v);
      ei = ei.ShiftLeft(16).Add(rand.UniformInt(0x10000));
      ei = ei.ShiftLeft(16).Add(rand.UniformInt(0x10000));
      ei = ei.ShiftLeft(16).Add(rand.UniformInt(0x10000));
      return ei;
    }

    public static EInteger RandomEIntegerMajorType0Or1(RandomGenerator rand) {
      int v = rand.UniformInt(0x10000);
      EInteger ei = EInteger.FromInt32(v);
      ei = ei.ShiftLeft(16).Add(rand.UniformInt(0x10000));
      ei = ei.ShiftLeft(16).Add(rand.UniformInt(0x10000));
      ei = ei.ShiftLeft(16).Add(rand.UniformInt(0x10000));
      if (rand.UniformInt(2) == 0) {
        ei = ei.Add(1).Negate();
      }
      return ei;
    }

    public static CBORObject RandomCBORTaggedObject(
      RandomGenerator rand,
      int depth) {
      var tag = 0;
      if (rand.UniformInt(2) == 0) {
        int[] tagselection = {
          2, 2, 2, 3, 3, 3, 4, 4, 4, 5, 5, 5, 30, 30,
          30, 0, 1, 25, 26, 27,
        };
        tag = tagselection[rand.UniformInt(tagselection.Length)];
      } else if (rand.UniformInt(100) < 90) {
        return CBORObject.FromObjectAndTag (
            RandomCBORObject(rand, depth + 1),
            rand.UniformInt(0x100000));
      } else {
        return CBORObject.FromObjectAndTag (
            RandomCBORObject(rand, depth + 1),
            RandomEIntegerMajorType0(rand));
      }
      if (tag == 25) {
        tag = 0;
      }
      if (tag == 30) {
        object o = RandomObjects.RandomByteString(rand);
        return ToObjectTest.TestToFromObjectRoundTrip(o);
      }
      {
        CBORObject cbor;
        // Console.WriteLine("tag "+tag+" "+i);
        if (tag == 0 || tag == 1 || tag == 28 || tag == 29) {
          tag = 999;
        }
        if (tag == 2 || tag == 3) {
          object o = RandomObjects.RandomByteStringShort(rand);
          cbor = ToObjectTest.TestToFromObjectRoundTrip(o);
        } else if (tag == 4 || tag == 5) {
          cbor = CBORObject.NewArray();
          object o = RandomObjects.RandomSmallIntegral(rand);
          cbor.Add(ToObjectTest.TestToFromObjectRoundTrip(o));
          o = RandomObjects.RandomEInteger(rand);
          cbor.Add(ToObjectTest.TestToFromObjectRoundTrip(o));
        } else if (tag == 30) {
          cbor = CBORObject.NewArray();
          object o = RandomObjects.RandomSmallIntegral(rand);
          cbor.Add(ToObjectTest.TestToFromObjectRoundTrip(o));
          o = RandomObjects.RandomEInteger(rand);
          cbor.Add(ToObjectTest.TestToFromObjectRoundTrip(o));
        } else {
          cbor = RandomCBORObject(rand, depth + 1);
        }
        return CBORObject.FromObjectAndTag(cbor, tag);
      }
    }

    public static CBORObject RandomCBORArray(RandomGenerator rand, int depth) {
      int x = rand.UniformInt(100);
      int count = (x < 80) ? 2 : ((x < 93) ? 1 : ((x < 98) ? 0 : 10));
      CBORObject cborRet = CBORObject.NewArray();
      for (var i = 0; i < count; ++i) {
        cborRet.Add(RandomCBORObject(rand, depth + 1));
      }
      return cborRet;
    }

    public static CBORObject RandomCBORObject(RandomGenerator rand) {
      return RandomCBORObject(rand, 0);
    }

    public static CBORObject RandomCBORObject(RandomGenerator rand, int
      depth) {
      int nextval = rand.UniformInt(11);
      switch (nextval) {
        case 0:
        case 1:
        case 2:
        case 3:
          return RandomNumberOrRational(rand);
        case 4:
          return rand.UniformInt(2) == 0 ? CBORObject.True : CBORObject.False;
        case 5:
          return rand.UniformInt(2) == 0 ? CBORObject.Null :
            CBORObject.Undefined;
        case 6:
          return ToObjectTest.TestToFromObjectRoundTrip (
              RandomObjects.RandomTextString(rand));
        case 7:
          return ToObjectTest.TestToFromObjectRoundTrip (
              RandomObjects.RandomByteString(rand));
        case 8:
          return RandomCBORArray(rand, depth);
        case 9:
          return RandomCBORMap(rand, depth);
        case 10:
          return RandomCBORTaggedObject(rand, depth);
        default: return RandomNumber(rand);
      }
    }

#pragma warning disable CS0618
    public static void TestNumber(CBORObject o) {
      if (o.Type != CBORType.Number) {
        return;
      }
      if (o.IsPositiveInfinity() || o.IsNegativeInfinity() ||
        o.IsNaN()) {
        try {
          o.AsByte();
          Assert.Fail("Should have failed");
        } catch (OverflowException) {
          // NOTE: Intentionally empty
        } catch (Exception ex) {
          Assert.Fail("Object: " + o + ", " + ex);
          throw new InvalidOperationException(String.Empty, ex);
        }
        try {
          o.AsInt16();
          Assert.Fail("Should have failed");
        } catch (OverflowException) {
          // NOTE: Intentionally empty
        } catch (Exception ex) {
          Assert.Fail("Object: " + o + ", " + ex);
          throw new InvalidOperationException(String.Empty, ex);
        }
        try {
          o.AsInt32();
          Assert.Fail("Should have failed");
        } catch (OverflowException) {
          // NOTE: Intentionally empty
        } catch (Exception ex) {
          Assert.Fail("Object: " + o + ", " + ex);
          throw new InvalidOperationException(String.Empty, ex);
        }
        try {
          o.AsInt64();
          Assert.Fail("Should have failed");
        } catch (OverflowException) {
          // NOTE: Intentionally empty
        } catch (Exception ex) {
          Assert.Fail("Object: " + o + ", " + ex);
          throw new InvalidOperationException(String.Empty, ex);
        }
        try {
          o.AsSingle();
        } catch (Exception ex) {
          Assert.Fail(ex.ToString());
          throw new InvalidOperationException(String.Empty, ex);
        }
        try {
          o.AsDouble();
        } catch (Exception ex) {
          Assert.Fail(ex.ToString());
          throw new InvalidOperationException(String.Empty, ex);
        }
        try {
          o.AsEInteger();
          Assert.Fail("Should have failed");
        } catch (OverflowException) {
          // NOTE: Intentionally empty
        } catch (Exception ex) {
          Assert.Fail("Object: " + o + ", " + ex);
          throw new InvalidOperationException(String.Empty, ex);
        }
        return;
      }
      try {
        o.AsSingle();
      } catch (Exception ex) {
        Assert.Fail("Object: " + o + ", " + ex);
        throw new InvalidOperationException(String.Empty, ex);
      }
      try {
        o.AsDouble();
      } catch (Exception ex) {
        Assert.Fail("Object: " + o + ", " + ex);
        throw new InvalidOperationException(String.Empty, ex);
      }
    }
#pragma warning restore CS0618

    public static byte[] CheckEncodeToBytes(CBORObject o) {
      byte[] bytes = o.EncodeToBytes();
      if (bytes.Length != o.CalcEncodedSize()) {
        string msg = "encoded size doesn't match:\no = " +
          TestCommon.ToByteArrayString(bytes) + "\nostring = " + o.ToString();
        Assert.AreEqual(bytes.Length, o.CalcEncodedSize(), msg);
      }
      return bytes;
    }

    public static void AssertRoundTrip(CBORObject o) {
      CBORObject o2 = FromBytesTestAB(CheckEncodeToBytes(o));
      TestCommon.CompareTestEqual(o, o2);
      TestNumber(o);
      TestCommon.AssertEqualsHashCode(o, o2);
    }

    public static void AssertJSONSer(CBORObject o, String s) {
      if (!s.Equals(o.ToJSONString(), StringComparison.Ordinal)) {
        Assert.AreEqual(s, o.ToJSONString(), "o is not equal to s");
      }
      byte[] bytes = CheckEncodeToBytes(o);
      // Test round-tripping
      CBORObject o2 = FromBytesTestAB(bytes);
      if (!s.Equals(o2.ToJSONString(), StringComparison.Ordinal)) {
        string msg = "o2 is not equal to s:\no = " +
          TestCommon.ToByteArrayString(bytes) +
          "\no2 = " + TestCommon.ToByteArrayString(o2.EncodeToBytes()) +
          "\no2string = " + o2.ToString();
        Assert.AreEqual(s, o2.ToJSONString(), msg);
      }
      TestNumber(o);
      TestCommon.AssertEqualsHashCode(o, o2);
    }

    // Tests the equivalence of the DecodeFromBytes and Read methods.
    public static CBORObject FromBytesTestAB(byte[] b) {
      CBORObject oa = FromBytesA(b);
      CBORObject ob = FromBytesB(b);
      if (!oa.Equals(ob)) {
        Assert.AreEqual(oa, ob);
      }
      return oa;
    }

    private static CBORObject FromBytesA(byte[] b) {
      return CBORObject.DecodeFromBytes(b);
    }

    private static CBORObject FromBytesB(byte[] b) {
      using (var ms = new System.IO.MemoryStream(b)) {
        CBORObject o = CBORObject.Read(ms);
        if (ms.Position != ms.Length) {
          throw new CBORException("not at EOF");
        }
        return o;
      }
    }
  }
}
