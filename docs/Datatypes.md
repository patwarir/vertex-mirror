# Datatypes

## Table of Contents
- [`vd`](#vd-void)
- [`bl`](#bl-boolean)
- [`int`](#int-integer)
- [`fl`](#fl-float)
- [`str`](#str-string)

## `vd` (Void)
**Buffer Value:** `0x0`

**Literal Values:** N/A

**Example(s):** N/A

## `bl` (Boolean)
**Buffer Value:** `0x1`

**Literal Values:** `false`, `true`

**Example(s):**
- `false`
- `true`

## `int` (Integer)
**Buffer Value:** `0x2`

**Literal Values:** Any 64-bit signed integer.

**Example(s):**
- `0`
- `+1`
- `23`
- `-45`

## `fl` (Float)
**Buffer Value:** `0x3`

**Literal Values:** Any 64-bit floating-point number.

**Example(s):**
- `0.0`
- `7.563`
- `+64.85`
- `-.3451`

## `str` (String)
**Buffer Value:** `0x4`

**Literal Values:** Any string of non-control ASCII characters surrounded by double quotes.

**Example(s):**
- `"a"`
- `"Test_123"`
- `"Hello, World!"`