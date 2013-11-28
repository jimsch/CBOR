package com.upokecenter.util;


import java.math.*;


    /**
     * Encapsulates radix-independent arithmetic.
     */
  class RadixMath<T> {

    IRadixMathHelper<T> helper;

    public RadixMath(IRadixMathHelper<T> helper) {
      this.helper = helper;
    }
    /**
     * Gets the lesser value between two T values.
     * @param a A T object.
     * @param b A T object.
     * @return The smaller value of the two objects.
     */
    public T Min(T a, T b) {
      if (a == null) throw new NullPointerException("a");
      if (b == null) throw new NullPointerException("b");
      int cmp = compareTo(a, b);
      if (cmp != 0)
        return cmp > 0 ? b : a;
      // Here the signs of both a and b can only be
      // equal (negative zeros are not supported)
      if (helper.GetSign(a) >= 0) {
        return (helper.GetExponent(a)).compareTo(helper.GetExponent(b)) > 0 ? b : a;
      } else {
        return (helper.GetExponent(a)).compareTo(helper.GetExponent(b)) > 0 ? a : b;
      }
    }


    private boolean Round(IShiftAccumulator accum, Rounding rounding,
                              boolean neg, BigInteger bigval) {
      boolean incremented = false;
      int radix = helper.GetRadix();
      if (rounding == Rounding.HalfUp) {
        if (accum.getLastDiscardedDigit() >= (radix / 2)) {
          incremented = true;
        }
      } else if (rounding == Rounding.HalfEven) {
        if (accum.getLastDiscardedDigit() >= (radix / 2)) {
          if ((accum.getLastDiscardedDigit() > (radix / 2) || accum.getOlderDiscardedDigits() != 0)) {
            incremented = true;
          } else if (bigval.testBit(0)) {
            incremented = true;
          }
        }
      } else if (rounding == Rounding.Ceiling) {
        if (!neg && (accum.getLastDiscardedDigit() | accum.getOlderDiscardedDigits()) != 0) {
          incremented = true;
        }
      } else if (rounding == Rounding.Floor) {
        if (neg && (accum.getLastDiscardedDigit() | accum.getOlderDiscardedDigits()) != 0) {
          incremented = true;
        }
      } else if (rounding == Rounding.HalfDown) {
        if (accum.getLastDiscardedDigit() > (radix / 2) ||
          (accum.getLastDiscardedDigit() == (radix / 2) && accum.getOlderDiscardedDigits() != 0)) {
          incremented = true;
        }
      } else if (rounding == Rounding.Up) {
        if ((accum.getLastDiscardedDigit() | accum.getOlderDiscardedDigits()) != 0) {
          incremented = true;
        }
      } else if (rounding == Rounding.ZeroFiveUp) {
        if ((accum.getLastDiscardedDigit() | accum.getOlderDiscardedDigits()) != 0) {
          if (radix == 2) {
            incremented = true;
          } else {
            BigInteger bigdigit = bigval.remainder(BigInteger.valueOf(radix));
            int lastDigit = bigdigit.intValue();
            if (lastDigit == 0 || lastDigit == (radix / 2)) {
              incremented = true;
            }
          }
        }
      }
      return incremented;
    }

    private T EnsureSign(T val, boolean negative) {
      if (val == null) return val;
      int sign = helper.GetSign(val);
      if (negative && sign > 0) {
        BigInteger bigmant = helper.GetMantissa(val);
        bigmant=(bigmant).negate();
        BigInteger e = helper.GetExponent(val);
        return helper.CreateNew(bigmant, e);
      } else if (!negative && sign < 0) {
        return helper.Abs(val);
      }
      return val;
    }

    /**
     * 
     * @param thisValue A T object.
     * @param divisor A T object.
     * @param ctx A PrecisionContext object.
     */
public T DivideToIntegerNaturalScale(
      T thisValue,
      T divisor,
      PrecisionContext ctx
      ) {
      T ret = DivideInternal(thisValue, divisor,
        new PrecisionContext(Rounding.Down),
        IntegerModeNaturalScale, BigInteger.ZERO);
      boolean neg = (helper.GetSign(thisValue) < 0) ^ (helper.GetSign(divisor) < 0);
      if (helper.GetExponent(ret).signum() < 0) {
        BigInteger bigmantissa = (helper.GetMantissa(ret)).abs();
        IShiftAccumulator accum = helper.CreateShiftAccumulator(bigmantissa);
        accum.ShiftRight(new FastInteger(helper.GetExponent(ret)).Negate());
        bigmantissa = accum.getShiftedInt();
        if (helper.GetMantissa(ret).signum() < 0) bigmantissa=bigmantissa.negate();
        ret = helper.CreateNew(bigmantissa, BigInteger.ZERO);
      }
      if (helper.GetSign(ret) == 0) {
        BigInteger divisorExp=helper.GetExponent(divisor);
        ret = helper.CreateNew(BigInteger.ZERO, helper.GetExponent(thisValue).subtract(divisorExp));
      } else {
        FastInteger desiredScale = new FastInteger(helper.GetExponent(thisValue)).Subtract(
          helper.GetExponent(divisor));
        if (desiredScale.signum() < 0) {
          desiredScale.Negate();
          BigInteger bigmantissa = (helper.GetMantissa(ret)).abs();
          bigmantissa = helper.MultiplyByRadixPower(bigmantissa, desiredScale);
          if (helper.GetMantissa(ret).signum() < 0) bigmantissa=bigmantissa.negate();
          ret = helper.CreateNew(bigmantissa, helper.GetExponent(thisValue).subtract(helper.GetExponent(divisor)));
        } else if (desiredScale.signum() > 0) {
          BigInteger bigmantissa = (helper.GetMantissa(ret)).abs();
          BigInteger bigexponent = helper.GetExponent(ret);
          BigInteger bigradix = BigInteger.valueOf(helper.GetRadix());
          while ((bigmantissa.remainder(bigradix)).signum()==0) {
            bigmantissa=bigmantissa.divide(bigradix);
            bigexponent=bigexponent.add(BigInteger.ONE);
          }
          if (helper.GetMantissa(ret).signum() < 0) bigmantissa=bigmantissa.negate();
          ret = helper.CreateNew(bigmantissa, bigexponent);
        }
      }
      if (ctx != null) {
        ret = RoundToPrecision(ret, ctx);
      }
      ret = EnsureSign(ret, neg);
      return ret;
    }

    /**
     * 
     * @param thisValue A T object.
     * @param divisor A T object.
     * @param ctx A PrecisionContext object.
     */
public T DivideToIntegerZeroScale(
      T thisValue,
      T divisor,
      PrecisionContext ctx
      ) {
      T ret = DivideInternal(thisValue, divisor,
        new PrecisionContext(Rounding.Down).WithPrecision(ctx == null ? 0 : ctx.getPrecision()),
        IntegerModeFixedScale, BigInteger.ZERO);
      if (ctx != null) {
        PrecisionContext ctx2 = ctx.WithBlankFlags().WithUnlimitedExponents();
        ret = RoundToPrecision(ret, ctx2);
        if ((ctx2.getFlags() & PrecisionContext.FlagRounded) != 0) {
          throw new ArithmeticException("Result would require a higher precision");
        }
      }
      return ret;
    }



    /**
     * Divides two T objects.
     * @param thisValue A T object.
     * @param divisor A T object.
     * @param desiredExponent A BigInteger object.
     * @param ctx A PrecisionContext object.
     * @return The quotient of the two objects.
     */
public T Divide(
      T thisValue,
          T divisor,
          BigInteger desiredExponent,
          PrecisionContext ctx
        ) {
      PrecisionContext ctx2 = (ctx == null) ?
        new PrecisionContext(Rounding.HalfDown) :
        ctx.WithUnlimitedExponents().WithPrecision(0);
      T ret = DivideInternal(thisValue, divisor,
        ctx2,
        IntegerModeFixedScale, desiredExponent);
      if (ctx != null && ctx.getHasFlags()) {
        ctx.setFlags(ctx.getFlags()|ctx2.getFlags());
      }
      return ret;
    }

    /**
     * Divides two T objects.
     * @param thisValue A T object.
     * @param divisor A T object.
     * @param ctx A PrecisionContext object.
     * @return The quotient of the two objects.
     */
    public T Divide(
          T thisValue,
              T divisor,
              PrecisionContext ctx
            ) {
      return DivideInternal(thisValue, divisor,
        ctx, IntegerModeRegular, BigInteger.ZERO);
    }

    private BigInteger RoundToScale(
      BigInteger mantissa, // Assumes mantissa is nonnegative
      BigInteger remainder,// Assumes value is nonnegative
      BigInteger divisor,// Assumes value is nonnegative
      FastInteger shift,
      boolean neg,
      PrecisionContext ctx
    ) {
      IShiftAccumulator accum;
      int lastDiscarded = 0;
      int olderDiscarded = 0;
      if (!(remainder.signum()==0)) {
        BigInteger halfDivisor = (divisor.shiftRight(1));
        int cmpHalf = remainder.compareTo(halfDivisor);
        if ((cmpHalf == 0) && divisor.testBit(0)==false) {
          // remainder is exactly half
          lastDiscarded = (helper.GetRadix() / 2);
          olderDiscarded = 0;
        } else if (cmpHalf > 0) {
          // remainder is greater than half
          lastDiscarded = (helper.GetRadix() / 2);
          olderDiscarded = 1;
        } else {
          // remainder is less than half
          lastDiscarded = 0;
          olderDiscarded = 1;
        }
      }
      accum = helper.CreateShiftAccumulator(
        mantissa, lastDiscarded, olderDiscarded);
      accum.ShiftRight(shift);
      int flags = 0;
      Rounding rounding = (ctx == null) ? Rounding.HalfEven : ctx.getRounding();
      BigInteger newmantissa = accum.getShiftedInt();
      if ((accum.getDiscardedDigitCount()).signum() != 0 ||
        (accum.getLastDiscardedDigit() | accum.getOlderDiscardedDigits()) != 0) {
        if (mantissa.signum()!=0)
          flags |= PrecisionContext.FlagRounded;
        if ((accum.getLastDiscardedDigit() | accum.getOlderDiscardedDigits()) != 0) {
          flags |= PrecisionContext.FlagInexact;
          if (rounding == Rounding.Unnecessary)
            throw new ArithmeticException("Rounding was required");
        }
        if (Round(accum, rounding, neg, newmantissa)) {
          newmantissa=newmantissa.add(BigInteger.ONE);
        }
      }
      if (ctx.getHasFlags()) {
        ctx.setFlags(ctx.getFlags()|flags);
      }
      if (neg) {
        newmantissa=newmantissa.negate();
      }
      return newmantissa;
    }

    private static final int IntegerModeFixedScale = 1;
    private static final int IntegerModeNaturalScale = 2;
    private static final int IntegerModeRegular = 0;

    private T DivideInternal(
      T thisValue,
          T divisor,
          PrecisionContext ctx,
          int integerMode,
          BigInteger desiredExponent
        ) {
      int signA = helper.GetSign(thisValue);
      int signB = helper.GetSign(divisor);
      if (signB == 0) {
        throw new ArithmeticException();
      }
      if (signA == 0) {
        if (integerMode == IntegerModeFixedScale) {
          return helper.CreateNew(BigInteger.ZERO, desiredExponent);
        } else {
          BigInteger divExp=helper.GetExponent(divisor);
          return RoundToPrecision(helper.CreateNew(
            BigInteger.ZERO, (helper.GetExponent(thisValue).subtract(divExp))), ctx);
        }
      } else {
        BigInteger mantissaDividend = helper.GetMantissa(thisValue);
        BigInteger mantissaDivisor = helper.GetMantissa(divisor);
        FastInteger adjust = new FastInteger();
        FastInteger2 result = new FastInteger2();
        boolean negA = (signA < 0);
        boolean negB = (signB < 0);
        if (negA) mantissaDividend=mantissaDividend.negate();
        if (negB) mantissaDivisor=mantissaDivisor.negate();
        FastInteger resultPrecision = new FastInteger(1);
        int mantcmp = mantissaDividend.compareTo(mantissaDivisor);
        if (mantcmp < 0) {
          // dividend mantissa is less than divisor mantissa
          long dividendPrecision = helper.CreateShiftAccumulator(mantissaDividend).getDigitLength();
          long divisorPrecision = helper.CreateShiftAccumulator(mantissaDivisor).getDigitLength();
          long powerOfRadix = Math.max(1, divisorPrecision - dividendPrecision);
          // multiply dividend mantissa so precisions are the same
          // (except if they're already the same, in which case multiply
          // by radix)
          mantissaDividend = helper.MultiplyByRadixPower(mantissaDividend, powerOfRadix);
          adjust.Add(powerOfRadix);
          if (mantissaDividend.compareTo(mantissaDivisor) < 0) {
            // dividend mantissa is still less, multiply once more
            mantissaDividend = helper.MultiplyByRadixPower(mantissaDividend, 1);
            adjust.Add(1);
          }
        } else if (mantcmp > 0) {
          // dividend mantissa is greater than divisor mantissa
          long dividendPrecision = helper.CreateShiftAccumulator(mantissaDividend).getDigitLength();
          long divisorPrecision = helper.CreateShiftAccumulator(mantissaDivisor).getDigitLength();
          long powerOfRadix = (dividendPrecision - divisorPrecision);
          BigInteger oldMantissaB = mantissaDivisor;
          mantissaDivisor = helper.MultiplyByRadixPower(mantissaDivisor, powerOfRadix);
          adjust.Subtract(powerOfRadix);
          if (mantissaDividend.compareTo(mantissaDivisor) < 0) {
            // dividend mantissa is now less, divide by radix power
            if (powerOfRadix == 1) {
              // no need to divide here, since thisValue would just undo
              // the multiplication
              mantissaDivisor = oldMantissaB;
            } else {
              BigInteger bigpow = BigInteger.valueOf(helper.GetRadix());
              mantissaDivisor=mantissaDivisor.divide(bigpow);
            }
            adjust.Add(1);
          }
        }
        FastInteger expdiff = new FastInteger(helper.GetExponent(thisValue)).Subtract(
          helper.GetExponent(divisor));
        FastInteger fastDesiredExponent = new FastInteger(desiredExponent);
        if (integerMode == IntegerModeFixedScale) {
          if (ctx != null && ctx.getPrecision() != 0 &&
          expdiff.compareTo(ctx.getPrecision()) > 0 &&
            desiredExponent.signum()==0) {
            // Result would require a too-high precision since
            // exponent difference is much higher
            throw new ArithmeticException("Result can't fit the precision");
          }
        }
        if (mantcmp == 0) {
          result = new FastInteger2().Add(1);
          mantissaDividend = BigInteger.ZERO;
        } else {
          int check = 0;
          while (true) {
            BigInteger remainder;
            BigInteger olddividend = mantissaDividend;
            BigInteger quotient;
BigInteger[] divrem=(mantissaDividend).divideAndRemainder(mantissaDivisor);
quotient=divrem[0];
remainder=divrem[1];
            result.Add(quotient.intValue());
            mantissaDividend = remainder;
            if (ctx != null && ctx.getPrecision() != 0 &&
              resultPrecision.compareTo(ctx.getPrecision()) == 0) {
              break;
            }
            if (remainder.signum()==0 && adjust.signum() >= 0) {
              break;
            }
            // NOTE: 5 is an arbitrary threshold
            if (check == 5 && (ctx == null || ctx.getPrecision() == 0) &&
              integerMode == IntegerModeRegular) {
              // Check for a non-terminating radix expansion
              // if using unlimited precision and not in integer
              // mode
              if (!helper.HasTerminatingRadixExpansion(olddividend, mantissaDivisor)) {
                throw new ArithmeticException("Result would have a nonterminating expansion");
              }
              check++;
            } else if (check < 5) {
              check++;
            }
            if (integerMode == IntegerModeFixedScale ||
              integerMode == IntegerModeNaturalScale) {
              if (new FastInteger(expdiff).Subtract(fastDesiredExponent).compareTo(adjust) <= 0) {
                // Value is a full integer or has a fractional part
                break;
              }
            }
            adjust.Add(1);
            if (result.signum() != 0) {
              resultPrecision.Add(1);
            }
            result.Multiply(helper.GetRadix());
            mantissaDividend = helper.MultiplyByRadixPower(mantissaDividend, 1);
          }
        }
        // mantissaA now has the remainder
        FastInteger exp = new FastInteger(expdiff).Subtract(adjust);
        if (integerMode == IntegerModeFixedScale) {
          FastInteger expshift = new FastInteger(exp).Subtract(fastDesiredExponent);
          if (result.signum() != 0) {
            if (expshift.signum() > 0) {
              // Exponent is greater than desired exponent
              if (ctx != null && ctx.getPrecision() != 0 &&
                   expshift.compareTo(new FastInteger(ctx.getPrecision()).Add(8)) > 0) {
                // Result would require a too-high precision since
                // exponent shift is much higher
                throw new ArithmeticException();
              }
              mantissaDividend = helper.MultiplyByRadixPower(result.AsBigInteger(), expshift);
              if (negA ^ negB) {
                mantissaDividend=mantissaDividend.negate();
              }
              return helper.CreateNew(mantissaDividend, desiredExponent);
            } else if (expshift.signum() < 0) {
              // Exponent is less than desired exponent
              expshift.Negate();
              if (expshift.compareTo(resultPrecision) > 0) {
                // Exponent minus desired exponent
                // is greater than the result's precision,
                // so the result would be reduced to zero
                if (ctx != null && ctx.getRounding() == Rounding.Down && !ctx.getHasFlags()) {
                  return helper.CreateNew(BigInteger.ZERO, desiredExponent);
                }
              }
              if (ctx != null && ctx.getRounding() == Rounding.Down && !ctx.getHasFlags()) {
                IShiftAccumulator accum = helper.CreateShiftAccumulator(result.AsBigInteger());
                accum.ShiftRight(expshift);
                mantissaDividend = accum.getShiftedInt();
                if (negA ^ negB) {
                  mantissaDividend=mantissaDividend.negate();
                }
                return helper.CreateNew(mantissaDividend, desiredExponent);
              } else {
                mantissaDividend = RoundToScale(
                  result.AsBigInteger(),
                  mantissaDividend,
                  mantissaDivisor,
                  expshift,
                  negA ^ negB,
                  ctx);
                return helper.CreateNew(mantissaDividend, desiredExponent);
              }
            } else {
              mantissaDividend = RoundToScale(
                                result.AsBigInteger(),
                                mantissaDividend,
                                mantissaDivisor,
                                expshift,
                                negA ^ negB,
                                ctx);
              return helper.CreateNew(mantissaDividend, desiredExponent);
            }
          } else if (ctx != null && ctx.getRounding() == Rounding.Down && !ctx.getHasFlags()) {
            return helper.CreateNew(BigInteger.ZERO, desiredExponent);
          }
        }
        int lastDiscarded = 0;
        int olderDiscarded = 0;
        if (!(mantissaDividend.signum()==0)) {
          BigInteger halfDivisor = (mantissaDivisor.shiftRight(1));
          int cmpHalf = mantissaDividend.compareTo(halfDivisor);
          if ((cmpHalf == 0) && mantissaDivisor.testBit(0)==false) {
            // remainder is exactly half
            lastDiscarded = (helper.GetRadix() / 2);
            olderDiscarded = 0;
          } else if (cmpHalf > 0) {
            // remainder is greater than half
            lastDiscarded = (helper.GetRadix() / 2);
            olderDiscarded = 1;
          } else {
            // remainder is less than half
            lastDiscarded = 0;
            olderDiscarded = 1;
          }
        }
        BigInteger bigResult = result.AsBigInteger();
        if (negA ^ negB) {
          bigResult=bigResult.negate();
        }
        return RoundToPrecision(
          helper.CreateNew(
          bigResult, exp.AsBigInteger()),
          ctx,
          lastDiscarded, olderDiscarded);
      }
    }

    /**
     * Gets the lesser value between two values, ignoring their signs. If
     * the absolute values are equal, has the same effect as Min.
     * @param a A T object.
     * @param b A T object.
     */
    public T MinMagnitude(T a, T b) {
      if (a == null) throw new NullPointerException("a");
      if (b == null) throw new NullPointerException("b");
      int cmp = compareTo(helper.Abs(a), helper.Abs(b));
      if (cmp == 0) return Min(a, b);
      return (cmp < 0) ? a : b;
    }
    /**
     * Gets the greater value between two values, ignoring their signs.
     * If the absolute values are equal, has the same effect as Max.
     * @param a A T object.
     * @param b A T object.
     */
    public T MaxMagnitude(T a, T b) {
      if (a == null) throw new NullPointerException("a");
      if (b == null) throw new NullPointerException("b");
      int cmp = compareTo(helper.Abs(a), helper.Abs(b));
      if (cmp == 0) return Max(a, b);
      return (cmp > 0) ? a : b;
    }
    /**
     * Gets the greater value between two T values.
     * @param a A T object.
     * @param b A T object.
     * @return The larger value of the two objects.
     */
    public T Max(T a, T b) {
      if (a == null) throw new NullPointerException("a");
      if (b == null) throw new NullPointerException("b");
      int cmp = compareTo(a, b);
      if (cmp != 0)
        return cmp > 0 ? a : b;
      // Here the signs of both a and b can only be
      // equal (negative zeros are not supported)
      if (helper.GetSign(a) >= 0) {
        return helper.GetExponent(a).compareTo(helper.GetExponent(b)) > 0 ? a : b;
      } else {
        return helper.GetExponent(a).compareTo(helper.GetExponent(b)) > 0 ? b : a;
      }
    }

    /**
     * Multiplies two T objects.
     * @param thisValue A T object.
     * @param decfrac A T object.
     * @param ctx A PrecisionContext object.
     * @return The product of the two objects.
     */
public T Multiply(T thisValue, T decfrac, PrecisionContext ctx) {
      BigInteger bigintOp2 = helper.GetExponent(decfrac);
      BigInteger newexp = (helper.GetExponent(thisValue).add(bigintOp2));
      T ret = helper.CreateNew(helper.GetMantissa(thisValue).multiply(helper.GetMantissa(decfrac)), newexp);
      if (ctx != null) {
        ret = RoundToPrecision(ret, ctx);
      }
      return ret;
    }
    /**
     * 
     * @param thisValue A T object.
     * @param multiplicand A T object.
     * @param augend A T object.
     * @param ctx A PrecisionContext object.
     */
    public T MultiplyAndAdd(T thisValue, T multiplicand,
                                              T augend,
                                              PrecisionContext ctx) {
      BigInteger bigintOp2 = helper.GetExponent(multiplicand);
      BigInteger newexp = (helper.GetExponent(thisValue).add(bigintOp2));
      bigintOp2 = helper.GetMantissa(multiplicand);
      bigintOp2=bigintOp2.multiply(helper.GetMantissa(thisValue));
      T addend = helper.CreateNew(bigintOp2,newexp);
      return Add(addend, augend, ctx);
    }



    /**
     * 
     * @param thisValue A T object.
     * @param context A PrecisionContext object.
     */
    public T RoundToPrecision(
          T thisValue,
          PrecisionContext context
         ) {
      return RoundToPrecision(thisValue, context, 0, 0);
    }
    private T RoundToPrecision(
      T thisValue,
      PrecisionContext context,
      int lastDiscarded,
      int olderDiscarded
     ) {
      if ((context) == null) return thisValue;
      if (context.getPrecision() == 0 && !context.getHasExponentRange() &&
        (lastDiscarded | olderDiscarded) == 0)
        return thisValue;
      FastInteger fastEMin = (context.getHasExponentRange()) ? new FastInteger(context.getEMin()) : null;
      FastInteger fastEMax = (context.getHasExponentRange()) ? new FastInteger(context.getEMax()) : null;
      if (context.getPrecision() > 0 && context.getPrecision() <= 18 &&
        (lastDiscarded | olderDiscarded) == 0) {
        // Check if rounding is necessary at all
        // for small precisions
        BigInteger mantabs = (helper.GetMantissa(thisValue)).abs();
        if (mantabs.compareTo(helper.MultiplyByRadixPower(BigInteger.ONE, context.getPrecision())) < 0) {
          if (!context.getHasExponentRange())
            return thisValue;
          FastInteger fastExp = new FastInteger(helper.GetExponent(thisValue));
          FastInteger fastAdjustedExp = new FastInteger(fastExp)
            .Add(context.getPrecision()).Subtract(1);
          FastInteger fastNormalMin = new FastInteger(fastEMin)
            .Add(context.getPrecision()).Subtract(1);
          if (fastAdjustedExp.compareTo(fastEMax) <= 0 &&
             fastAdjustedExp.compareTo(fastNormalMin) >= 0) {
            return thisValue;
          }
        }
      }
      int[] signals = new int[1];
      T dfrac = RoundToPrecisionInternal(
        thisValue,
        context.getPrecision(),
        context.getRounding(), fastEMin, fastEMax,
        lastDiscarded,
        olderDiscarded,
        signals);
      if (context.getClampNormalExponents() && dfrac != null) {
        // Clamp exponents to eMax + 1 - precision
        // if directed
        FastInteger clamp = new FastInteger(fastEMax).Add(1).Subtract(context.getPrecision());
        FastInteger fastExp = new FastInteger(helper.GetExponent(dfrac));
        if (fastExp.compareTo(clamp) > 0) {
          BigInteger bigmantissa = helper.GetMantissa(dfrac);
          int sign = bigmantissa.signum();
          if (sign != 0) {
            if (sign < 0) bigmantissa=bigmantissa.negate();
            FastInteger expdiff = new FastInteger(fastExp).Subtract(clamp);
            bigmantissa = helper.MultiplyByRadixPower(bigmantissa, expdiff);
            if (sign < 0) bigmantissa=bigmantissa.negate();
          }
          if (signals != null)
            signals[0] |= PrecisionContext.FlagClamped;
          dfrac = helper.CreateNew(bigmantissa, clamp.AsBigInteger());
        }
      }
      if (context.getHasFlags()) {
        context.setFlags(context.getFlags()|signals[0]);
      }
      return dfrac;
    }

    /**
     * 
     * @param thisValue A T object.
     * @param otherValue A T object.
     * @param ctx A PrecisionContext object.
     */
public T Quantize(
          T thisValue,
          T otherValue,
          PrecisionContext ctx
          ) {
      BigInteger mantThis = (helper.GetMantissa(thisValue)).abs();
      BigInteger expThis = helper.GetExponent(thisValue);
      BigInteger expOther = helper.GetExponent(otherValue);
      int expcmp = expThis.compareTo(expOther);
      int signThis = helper.GetSign(thisValue);
      PrecisionContext tmpctx = (ctx == null ?
        new PrecisionContext(Rounding.HalfEven) :
        new PrecisionContext(ctx)).WithBlankFlags();
      T ret = null;
      if (expcmp == 0) {
        ret = RoundToPrecision(thisValue, tmpctx);
      } else if (mantThis.signum()==0) {
        ret = helper.CreateNew(BigInteger.ZERO, expOther);
        ret = RoundToPrecision(ret, tmpctx);
      } else if (expcmp > 0) {
        // Other exponent is less
        FastInteger radixPower = new FastInteger(expThis).Subtract(expOther);
        if (tmpctx.getPrecision() > 0 &&
          radixPower.compareTo(new FastInteger(tmpctx.getPrecision()).Add(10)) > 0) {
          // Radix power is much too high for the current precision
          throw new ArithmeticException();
        }
        mantThis = helper.MultiplyByRadixPower(mantThis, radixPower);
        if (signThis < 0)
          mantThis=mantThis.negate();
        ret = helper.CreateNew(mantThis, expOther);
        ret = RoundToPrecision(ret, tmpctx);
      } else {
        // Other exponent is greater
        IShiftAccumulator accum = helper.CreateShiftAccumulator(mantThis);
        accum.ShiftRight(new FastInteger(expOther).Subtract(expThis));
        mantThis = accum.getShiftedInt();
        if (signThis < 0)
          mantThis=mantThis.negate();
        ret = helper.CreateNew(mantThis, expOther);
        ret = RoundToPrecision(ret, tmpctx, accum.getLastDiscardedDigit(),
          accum.getOlderDiscardedDigits());
      }
      if (ret == null || !helper.GetExponent(ret).equals(expOther)) {
        throw new ArithmeticException();
      }
      if ((tmpctx.getFlags() & PrecisionContext.FlagOverflow) != 0) {
        throw new ArithmeticException();
      }
      if (signThis < 0 && helper.GetSign(ret) > 0) {
        BigInteger mantRet = helper.GetMantissa(ret);
        mantRet=(mantRet).negate();
        ret = helper.CreateNew(mantRet,helper.GetExponent(ret));
      }
      if (ctx != null && ctx.getHasFlags()) {
        int flags = tmpctx.getFlags();
        flags &= ~PrecisionContext.FlagUnderflow;
        ctx.setFlags(ctx.getFlags()|flags);
      }
      return ret;
    }

    /**
     * 
     * @param thisValue A T object.
     * @param ctx A PrecisionContext object.
     */
public T RoundToIntegralExact(
      T thisValue,
      PrecisionContext ctx) {
      if (helper.GetExponent(thisValue).signum() >= 0) {
        return RoundToPrecision(thisValue, ctx);
      } else {
        PrecisionContext pctx = (ctx == null) ? null : 
          ctx.WithPrecision(0).WithBlankFlags();
        T ret = Quantize(thisValue, helper.CreateNew(
          BigInteger.ONE, BigInteger.ZERO),
          pctx);
        if (ctx != null && ctx.getHasFlags()) {
          ctx.setFlags(ctx.getFlags()|pctx.getFlags());
        }
        return ret;
      }
    }

    /**
     * 
     * @param thisValue A T object.
     * @param ctx A PrecisionContext object.
     */
public T RoundToIntegralValue(
  T thisValue,
  PrecisionContext ctx
  ) {
  PrecisionContext pctx = (ctx == null) ? null :
      ctx.WithBlankFlags();
  T ret = RoundToIntegralExact(thisValue, pctx);
  if (ctx != null && ctx.getHasFlags()) {
      ctx.setFlags(ctx.getFlags()|(pctx.getFlags()&~(PrecisionContext.FlagInexact|PrecisionContext.FlagRounded)));
  }
  return ret;
}

    private T RoundToPrecisionInternal(
      T thisValue,
      long precision,
      Rounding rounding,
      FastInteger fastEMin,
      FastInteger fastEMax,
      int lastDiscarded,
      int olderDiscarded,
      int[] signals
     ) {
      if ((precision) < 0) throw new IllegalArgumentException("precision" + " not greater or equal to " + "0" + " (" + Long.toString((long)(precision)) + ")");
      boolean neg = helper.GetMantissa(thisValue).signum() < 0;
      BigInteger bigmantissa = helper.GetMantissa(thisValue);
      if (neg) bigmantissa=bigmantissa.negate();
      // save mantissa in case result is subnormal
      // and must be rounded again
      BigInteger oldmantissa = bigmantissa;
      FastInteger exp = new FastInteger(helper.GetExponent(thisValue));
      int flags = 0;
      IShiftAccumulator accum = helper.CreateShiftAccumulator(
        bigmantissa, lastDiscarded, olderDiscarded);
      boolean unlimitedPrec = (precision == 0);
      if (precision > 0) {
        accum.ShiftToDigits(precision);
      } else {
        precision = accum.getDigitLength();
      }
      FastInteger discardedBits = new FastInteger(accum.getDiscardedDigitCount());
      exp.Add(discardedBits);
      FastInteger adjExponent = new FastInteger(exp)
        .Add(accum.getDigitLength()).Subtract(1);
      FastInteger clamp = null;
      if (fastEMax != null && adjExponent.compareTo(fastEMax) > 0) {
        if (oldmantissa.signum()==0) {
          flags |= PrecisionContext.FlagClamped;
          if (signals != null) signals[0] = flags;
          return helper.CreateNew(oldmantissa, fastEMax.AsBigInteger());
        }
        // Overflow
        flags |= PrecisionContext.FlagOverflow | PrecisionContext.FlagInexact | PrecisionContext.FlagRounded;
        if (rounding == Rounding.Unnecessary)
          throw new ArithmeticException("Rounding was required");
        if (!unlimitedPrec &&
           (rounding == Rounding.Down ||
            rounding == Rounding.ZeroFiveUp ||
            (rounding == Rounding.Ceiling && neg) ||
            (rounding == Rounding.Floor && !neg))) {
          // Set to the highest possible value for
          // the given precision
          BigInteger overflowMant = helper.MultiplyByRadixPower(BigInteger.ONE, precision);
          overflowMant=overflowMant.subtract(BigInteger.ONE);
          if (neg) overflowMant=overflowMant.negate();
          if (signals != null) signals[0] = flags;
          clamp = new FastInteger(fastEMax).Add(1).Subtract(precision);
          return helper.CreateNew(overflowMant, clamp.AsBigInteger());
        }
        if (signals != null) signals[0] = flags;
        return null;
      } else if (fastEMin != null && adjExponent.compareTo(fastEMin) < 0) {
        // Subnormal
        FastInteger fastETiny = new FastInteger(fastEMin)
          .Subtract(precision)
          .Add(1);
        if (oldmantissa.signum()!=0)
          flags |= PrecisionContext.FlagSubnormal;
        if (exp.compareTo(fastETiny) < 0) {
          FastInteger expdiff = new FastInteger(fastETiny).Subtract(exp);
          expdiff.Add(discardedBits);
          accum = helper.CreateShiftAccumulator(oldmantissa, lastDiscarded, olderDiscarded);
          accum.ShiftRight(expdiff);
          BigInteger newmantissa = accum.getShiftedInt();
          if ((accum.getDiscardedDigitCount()).signum() != 0 ||
            (accum.getLastDiscardedDigit() | accum.getOlderDiscardedDigits()) != 0) {
            if (oldmantissa.signum()!=0)
              flags |= PrecisionContext.FlagRounded;
            if ((accum.getLastDiscardedDigit() | accum.getOlderDiscardedDigits()) != 0) {
              flags |= PrecisionContext.FlagInexact;
              if (rounding == Rounding.Unnecessary)
                throw new ArithmeticException("Rounding was required");
            }
            if (Round(accum, rounding, neg, newmantissa)) {
              newmantissa=newmantissa.add(BigInteger.ONE);
            }
          }
          if (newmantissa.signum()==0)
            flags |= PrecisionContext.FlagClamped;
          if ((flags & (PrecisionContext.FlagSubnormal | PrecisionContext.FlagInexact)) == (PrecisionContext.FlagSubnormal | PrecisionContext.FlagInexact))
            flags |= PrecisionContext.FlagUnderflow | PrecisionContext.FlagRounded;
          if (signals != null) signals[0] = flags;
          if (neg) newmantissa=newmantissa.negate();
          return helper.CreateNew(newmantissa, fastETiny.AsBigInteger());
        }
      }
      boolean expChanged = false;
      if ((accum.getDiscardedDigitCount()).signum() != 0 ||
        (accum.getLastDiscardedDigit() | accum.getOlderDiscardedDigits()) != 0) {
        if (bigmantissa.signum()!=0)
          flags |= PrecisionContext.FlagRounded;
        bigmantissa = accum.getShiftedInt();
        if ((accum.getLastDiscardedDigit() | accum.getOlderDiscardedDigits()) != 0) {
          flags |= PrecisionContext.FlagInexact;
          if (rounding == Rounding.Unnecessary)
            throw new ArithmeticException("Rounding was required");
        }
        if (Round(accum, rounding, neg, bigmantissa)) {
          bigmantissa=bigmantissa.add(BigInteger.ONE);
          if (bigmantissa.testBit(0)==false) {
            accum = helper.CreateShiftAccumulator(bigmantissa);
            accum.ShiftToDigits(precision);
            if ((accum.getDiscardedDigitCount()).signum() != 0) {
              exp.Add(accum.getDiscardedDigitCount());
              discardedBits.Add(accum.getDiscardedDigitCount());
              bigmantissa = accum.getShiftedInt();
              expChanged = true;
            }
          }
        }
      }
      if (expChanged && fastEMax != null) {
        // If exponent changed, check for overflow again
        adjExponent = new FastInteger(exp);
        adjExponent.Add(accum.getDigitLength()).Subtract(1);
        if (adjExponent.compareTo(fastEMax) > 0) {
          flags |= PrecisionContext.FlagOverflow | PrecisionContext.FlagInexact | PrecisionContext.FlagRounded;
          if (!unlimitedPrec &&
             (rounding == Rounding.Down ||
              rounding == Rounding.ZeroFiveUp ||
              (rounding == Rounding.Ceiling && neg) ||
              (rounding == Rounding.Floor && !neg))) {
            // Set to the highest possible value for
            // the given precision
            BigInteger overflowMant = helper.MultiplyByRadixPower(BigInteger.ONE, precision);
            overflowMant=overflowMant.subtract(BigInteger.ONE);
            if (neg) overflowMant=overflowMant.negate();
            if (signals != null) signals[0] = flags;
            clamp = new FastInteger(fastEMax).Add(1).Subtract(precision);
            return helper.CreateNew(overflowMant, clamp.AsBigInteger());
          }
          if (signals != null) signals[0] = flags;
          return null;
        }
      }
      if (signals != null) signals[0] = flags;
      if (neg) bigmantissa=bigmantissa.negate();
      return helper.CreateNew(bigmantissa, exp.AsBigInteger());
    }

    /**
     * 
     * @param thisValue A T object.
     * @param decfrac A T object.
     * @param ctx A PrecisionContext object.
     */
public T Add(T thisValue, T decfrac, PrecisionContext ctx) {
      int expcmp = helper.GetExponent(thisValue).compareTo(helper.GetExponent(decfrac));
      T retval = null;
      if (expcmp == 0) {
        retval = helper.CreateNew(
          helper.GetMantissa(thisValue).add(helper.GetMantissa(decfrac)), helper.GetExponent(thisValue));
      } else {
        // choose the minimum exponent
        BigInteger resultExponent = (expcmp < 0 ? helper.GetExponent(thisValue) : helper.GetExponent(decfrac));
        T op1 = thisValue;
        T op2 = decfrac;
        BigInteger op1Exponent = helper.GetExponent(op1);
        BigInteger op2Exponent = helper.GetExponent(op2);
        BigInteger expdiff = (op1Exponent.subtract(op2Exponent)).abs();
        if (ctx != null && ctx.getPrecision() > 0) {
          // Check if exponent difference is too big for
          // radix-power calculation to work quickly
          if (expdiff.compareTo(BigInteger.valueOf(100)) >= 0) {
            FastInteger fastint = new FastInteger(expdiff).Add(3);
            // If exponent difference plus 3 is greater than the precision
            if (fastint.compareTo(ctx.getPrecision()) > 0) {
              int expcmp2 = op1Exponent.compareTo(op2Exponent);
              if (expcmp2 < 0 && !(helper.GetMantissa(op2).signum()==0)) {
                // first operand's exponent is less
                // and second operand isn't zero
                // the 8 digits at the end are guard digits
                op1Exponent = (new FastInteger(op2Exponent).Subtract(ctx.getPrecision()).Subtract(8)
                  .AsBigInteger());
              } else if (expcmp2 > 0 && !(helper.GetMantissa(op1).signum()==0)) {
                // first operand's exponent is greater
                // and first operand isn't zero
                // the 8 digits at the end are guard digits
                op2Exponent = (new FastInteger(op1Exponent).Subtract(ctx.getPrecision()).Subtract(8)
                  .AsBigInteger());
              }
              expcmp = op1Exponent.compareTo(op2Exponent);
              resultExponent = (expcmp < 0 ? op1Exponent : op2Exponent);
            }
          }
        }
        if (expcmp > 0) {
          BigInteger newmant = helper.RescaleByExponentDiff(
            helper.GetMantissa(op1), op1Exponent, op2Exponent);
          retval = helper.CreateNew(
            newmant.add(helper.GetMantissa(op2)), resultExponent);
        } else {
          BigInteger newmant = helper.RescaleByExponentDiff(
            helper.GetMantissa(op2), op1Exponent, op2Exponent);
          retval = helper.CreateNew(
            newmant.add(helper.GetMantissa(op1)), resultExponent);
        }
      }
      if (ctx != null) {
        retval = RoundToPrecision(retval, ctx);
      }
      return retval;
    }

    /**
     * Compares a T object with this instance.
     * @param thisValue A T object.
     * @param decfrac A T object.
     * @return Zero if the values are equal; a negative number is this instance
     * is less, or a positive number if this instance is greater.
     */
public int compareTo(T thisValue, T decfrac) {
      if (decfrac == null) return 1;
      int s = helper.GetSign(thisValue);
      int ds = helper.GetSign(decfrac);
      if (s != ds) return (s < ds) ? -1 : 1;
      int expcmp = helper.GetExponent(thisValue).compareTo(helper.GetExponent(decfrac));
      int mantcmp = helper.GetMantissa(thisValue).compareTo(helper.GetMantissa(decfrac));
      if (mantcmp == 0) {
        // Special case: Mantissas are equal
        return s == 0 ? 0 : expcmp * s;
      }
      if (ds == 0) {
        // Special case: Second operand is zero
        return s;
      }
      if (s == 0) {
        // Special case: First operand is zero
        return -ds;
      }
      if (expcmp == 0) {
        return mantcmp;
      } else {
        BigInteger op1Exponent = helper.GetExponent(thisValue);
        BigInteger op2Exponent = helper.GetExponent(decfrac);
        BigInteger expdiff = (op1Exponent.subtract(op2Exponent)).abs();
        // Check if exponent difference is too big for
        // radix-power calculation to work quickly
        if (expdiff.compareTo(BigInteger.valueOf(100)) >= 0) {
          FastInteger fastint = new FastInteger(expdiff).Add(3);
          long precision1 = helper.CreateShiftAccumulator(
            (helper.GetMantissa(thisValue)).abs()).getDigitLength();
          long precision2 = helper.CreateShiftAccumulator(
            (helper.GetMantissa(decfrac)).abs()).getDigitLength();
          long maxPrecision = Math.max(precision1, precision2);
          // If exponent difference plus 3 is greater than the
          // maximum precision of the two operands
          if (fastint.compareTo(maxPrecision) > 0) {
            int expcmp2 = op1Exponent.compareTo(op2Exponent);
            if (expcmp2 < 0) {
              // first operand's exponent is less
              // (second operand won't be zero at thisValue point)
              // the 8 digits at the end are guard digits
              op1Exponent = (new FastInteger(op2Exponent).Subtract(maxPrecision).Subtract(8)
                           .AsBigInteger());
            } else if (expcmp2 > 0) {
              // first operand's exponent is greater
              // (first operand won't be zero at thisValue point)
              // the 8 digits at the end are guard digits
              op2Exponent = (new FastInteger(op1Exponent).Subtract(maxPrecision).Subtract(8)
                           .AsBigInteger());
            }
            expcmp = op1Exponent.compareTo(op2Exponent);
          }
        }
        if (expcmp > 0) {
          BigInteger newmant = helper.RescaleByExponentDiff(
            helper.GetMantissa(thisValue), op1Exponent, op2Exponent);
          return newmant.compareTo(helper.GetMantissa(decfrac));
        } else {
          BigInteger newmant = helper.RescaleByExponentDiff(
            helper.GetMantissa(decfrac), op1Exponent, op2Exponent);
          return helper.GetMantissa(thisValue).compareTo(newmant);
        }
      }
    }


  }

