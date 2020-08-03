# Datatypes

## Table of Contents
- [`vd`](#vd-void)
- [`bl`](#bl-boolean)
- [`int`](#int-integer)
- [`fl`](#fl-float)
- [`str`](#str-string)

## `vd` (Void)
**Buffer Value:** `0x00`

**Summary:** The absence of a datatype.

**Literal Values:** N/A

**Example(s):** N/A

## `bl` (Boolean)
**Buffer Value:** `0x01`

**Summary:** A simple binary (`false`/`true`).

**Literal Values:** `false`, `true`

**Example(s):**
- `false`
- `true`

## `int` (Integer)
**Buffer Value:** `0x02`

**Summary:** A number without decimals.

**Literal Values:** Any 64-bit signed integer.

**Example(s):**
- `0`
- `+1`
- `23`
- `-45`

## `fl` (Float)
**Buffer Value:** `0x03`

**Summary:** A number with decimals.

**Literal Values:** Any 64-bit floating-point number.

**Example(s):**
- `0.0`
- `7.563`
- `+64.85`
- `-.3451`

## `str` (String)
**Buffer Value:** `0x04`

**Summary:** A string of characters.

**Literal Values:** Any string of non-control UTF-8 characters surrounded by double quotes.

**Example(s):**
- `"a"`
- `"Test_123"`
- `"Hello, World!"`