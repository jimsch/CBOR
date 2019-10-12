using System;
using NUnit.Framework;
using PeterO;
using PeterO.Cbor;
using PeterO.Numbers;

namespace Test {
  [TestFixture]
  public class CBORDataUtilitiesTest {
    private void AssertNegative(CBORObject obj) {
      Assert.IsTrue(obj.IsNegative);
      CBORTestCommon.AssertRoundTrip(obj);
    }
    [Test]
    public void TestPreserveNegativeZero() {
      CBORObject cbor;
      cbor = CBORDataUtilities.ParseJSONNumber("-0", false, false, true);
      {
        string stringTemp = cbor.ToObject(typeof(EDecimal)).ToString();
        Assert.AreEqual(
          "-0",
          stringTemp);
      }
      cbor = CBORDataUtilities.ParseJSONNumber("-0e-1", false, false, true);
      {
        string stringTemp = cbor.ToObject(typeof(EDecimal)).ToString();
        Assert.AreEqual(
          "-0.0",
          stringTemp);
      }
      cbor = CBORDataUtilities.ParseJSONNumber("-0e1", false, false, true);
      {
        string stringTemp = cbor.ToObject(typeof(EDecimal)).ToString();
        Assert.AreEqual(
          "-0E+1",
          stringTemp);
      }
      cbor = CBORDataUtilities.ParseJSONNumber("-0.0e1", false, false, true);
      {
        string stringTemp = cbor.ToObject(typeof(EDecimal)).ToString();
        Assert.AreEqual(
          "-0",
          stringTemp);
      }
      cbor = CBORDataUtilities.ParseJSONNumber("-0.0", false, false, true);
      {
        string stringTemp = cbor.ToObject(typeof(EDecimal)).ToString();
        Assert.AreEqual(
          "-0.0",
          stringTemp);
      }
      var assertNegatives = new string[] {
        "-0",
        "-0.0",
        "-0.0000",
        "-0e0",
        "-0e+1",
        "-0e-1",
        "-0e+999999999999",
        "-0e-999999999999",
        "-0.0e0",
        "-0.0e+1",
        "-0.0e-1",
        "-0.0e+999999999999",
        "-0.0e-999999999999",
        "-0.000e0",
        "-0.000e+0",
        "-0.000e-0",
        "-0.000e1",
        "-0.000e+1",
        "-0.000e-1",
        "-0.000e+999999999999",
        "-0.000e-999999999999",
      };
      foreach (var str in assertNegatives) {
        cbor = CBORDataUtilities.ParseJSONNumber(str, false, false, true);
        this.AssertNegative(cbor);
      }
    }
    [Test]
    public void TestParseJSONNumberNegativeZero() {
      var strings = new string[] {
        "-0", "0", "-0E+0", "0", "-0E-0", "0", "-0E-1", "0.0",
        "-0.00", "0.00", "-0.00E+0", "0.00", "-0.00E-0", "0.00",
        "-0.00E-1", "0.000",
      };
      for (var i = 0; i < strings.Length; i += 2) {
        var jsonDecimal = (EDecimal)CBORDataUtilities
                  .ParseJSONNumber(strings[i]).ToObject(typeof(EDecimal));
        Assert.AreEqual(
          strings[i + 1],
          jsonDecimal.ToString());
      }
    }

    [Test]
    public void TestParseJSONNumber() {
string[] badNumbers = {
  null, "100.", "-100.","100.e+20","-100.e+20",
  "100.e20", "+0.1", "0.","-0.","+0",
  "=0g.1", "0g.1", "0.e+20","-0.e20","-0.e+20",
  "0.e20", String.Empty, "xyz", "Infinity", "-Infinity",
  "true", ".1", ".01","-.1","-.01","-xyz","-true",
  "0..1", "-0..1", "0xyz","-0xyz",
  "0.1xyz", "0.xyz", "0.5exyz","0.5q+88",
  "0.5ee88", "-5e", "5e","88ee99",
  "-5e-2x", "-5e+2x", "5e-2x","5e+2x",
  "0.5e+xyz", "0.5e+88xyz",
  "00000", "00.5e+2", "00.5","00.5e-2","00.5e-999","00.5e999",
  "00000", "00.5E+2", "00.5","00.5E-2","00.5E-999","00.5e999",
  "00001", "01.5e+2", "01.5","01.5e-2","01.5e-999","01.5e999",
  "00001", "01.5E+2", "01.5","01.5E-2","01.5E-999","01.5e999",
  "--1", "--0", "--1.5E+2","--1.5","--1.5E-2","--1.5E-999","--1.5E999",
  "-00000", "-00.5e+2", "-00.5","-00.5e-2","-00.5e-999","-00.5e999",
  "-00000", "-00.5E+2", "-00.5","-00.5E-2","-00.5E-999","-00.5E999",
  "-00001", "-01.5e+2", "-01.5","-01.5e-2","-01.5e-999","-01.5e999",
  "-00001", "-01.5E+2", "-01.5","-01.5E-2","-01.5E-999","-01.5E999",
  "0x1", "0xf", "0x20","0x01",".2",".05",
  "-0x1", "-0xf", "-0x20","-0x01","-.2","-.05",
  "23.", "23.e-2", "23.e0","23.e1","0.",
  "5.2", "5e+1", "-5.2","-5e+1"
};
foreach (var str in badNumbers) {
  if (CBORDataUtilities.ParseJSONNumber(str) != null) {
    Assert.Fail(str);
  }
  if (CBORDataUtilities.ParseJSONNumber(str, false, false) != null) {
    Assert.Fail(str);
  }
  if (CBORDataUtilities.ParseJSONNumber(str, false, false, true) != null) {
    Assert.Fail(str);
  }
 if (CBORDataUtilities.ParseJSONNumber(str, false, false, false) != null) {
    Assert.Fail(str);
  }
 if (!Double.IsNaN(CBORDataUtilities.ParseJSONDouble(str))) {
   Assert.Fail(str);
 }
 if (!Double.IsNaN(CBORDataUtilities.ParseJSONDouble(str, false))) {
   Assert.Fail(str);
 }
}
CBORObject cbor = CBORDataUtilities.ParseJSONNumber("2e-2147483648");
CBORTestCommon.AssertJSONSer(cbor, "2E-2147483648");
TestCommon.CompareTestEqual(
  ToObjectTest.TestToFromObjectRoundTrip(230).AsNumber(),
  CBORDataUtilities.ParseJSONNumber("23.0e01").AsNumber());
  TestCommon.CompareTestEqual(
  ToObjectTest.TestToFromObjectRoundTrip(23).AsNumber(),
  CBORDataUtilities.ParseJSONNumber("23.0e00").AsNumber());
  cbor = CBORDataUtilities.ParseJSONNumber(
    "1e+99999999999999999999999999",
    false,
    false);
        Assert.IsTrue(cbor != null);
        Assert.IsFalse(cbor.CanFitInDouble());
        CBORTestCommon.AssertRoundTrip(cbor);
    }
  }
}
