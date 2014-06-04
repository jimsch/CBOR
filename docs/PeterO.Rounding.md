﻿## PeterO.Rounding

    public sealed struct Rounding :
        System.Enum,
        System.IComparable,
        System.IFormattable,
        System.IConvertible

Specifies the mode to use when "shortening" numbers that otherwise can't fit a given number of digits, so that the shortened number has about the same value. This "shortening" is known as rounding.

### Up

    public static PeterO.Rounding Up = 0;

If there is a fractional part, the number is rounded to the closest representable number away from zero.

### Down

    public static PeterO.Rounding Down = 1;

The fractional part is discarded (the number is truncated).

### Ceiling

    public static PeterO.Rounding Ceiling = 2;

If there is a fractional part, the number is rounded to the highest representable number that's closest to it.

### Floor

    public static PeterO.Rounding Floor = 3;

If there is a fractional part, the number is rounded to the lowest representable number that's closest to it.

### HalfUp

    public static PeterO.Rounding HalfUp = 4;

Rounded to the nearest number; if the fractional part is exactly half, the number is rounded to the closest representable number away from zero. This is the most familiar rounding mode for many people.

### HalfDown

    public static PeterO.Rounding HalfDown = 5;

Rounded to the nearest number; if the fractional part is exactly half, it is discarded.

### HalfEven

    public static PeterO.Rounding HalfEven = 6;

Rounded to the nearest number; if the fractional part is exactly half, the number is rounded to the closest representable number that is even. This is sometimes also known as "banker's rounding".

### Unnecessary

    public static PeterO.Rounding Unnecessary = 7;

Indicates that rounding will not be used. If rounding is required, the rounding operation will report an error.

### ZeroFiveUp

    public static PeterO.Rounding ZeroFiveUp = 8;

If there is a fractional part and if the last digit before rounding is 0 or half the radix, the number is rounded to the closest representable number away from zero; otherwise the fractional part is discarded. In overflow, the fractional part is always discarded. This rounding mode is useful for rounding intermediate results at a slightly higher precision than the final precision.

