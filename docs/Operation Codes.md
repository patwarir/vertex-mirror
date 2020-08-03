# Operation Codes

## Table of Contents
- [`nop`](#nop-no-operation)
- [`jmp.a`](#jmpa-jump-always)
- [`jmp.f`](#jmpf-jump-false)
- [`jmp.t`](#jmpt-jump-true)
- [`call`](#call-call)
- [`ret`](#ret-return)
- [`pop`](#pop-stack-pop)
- [`dup`](#dup-stack-duplicate)
- [`clr`](#clr-stack-clear)
- [`ld.lt`](#ldlt-load-literal)
- [`ld.gl`](#ldgl-load-global)
- [`ld.pr`](#ldpr-load-parameter)
- [`ld.lc`](#ldlc-load-local)
- [`st.lc`](#stlc-set-local)
- [`break`](#break-break)

## `nop` (No Operation)
**Buffer Value:** `0x00`

**Summary:** Does nothing.

**Layout and Parameter(s):**
```
nop;
```

**Example(s):**
```
nop;
```

**Stack:** N/A

**Position:** Next

## `jmp.a` (Jump Always)
**Buffer Value:** `0x01`

**Summary:** Jumps to the given label always.

**Layout and Parameter(s):**
```
jmp.a {0};
```
- `{0}` = Label identifier

**Example(s):**
```
jmp.a LOOP_BEGIN;
```

**Stack:** N/A

**Position:** Jumps to the given label.

## `jmp.f` (Jump False)
**Buffer Value:** `0x02`

**Summary:** Jumps to the given label when the value on top of the stack is `false`.

**Layout and Parameter(s):**
```
jmp.f {0};
```
- `{0}` = Label identifier

**Example(s):**
```
jmp.f LOOP_END;
```

**Stack:** Pop 1

**Position:** If the value on top of the stack is `false`, jumps to the given label, else next.

## `jmp.t` (Jump True)
**Buffer Value:** `0x03`

**Summary:** Jumps to the given label when the value on top of the stack is `true`.

**Layout and Parameter(s):**
```
jmp.t {0};
```
- `{0}` = Label identifier

**Example(s):**
```
jmp.t LOOP_END;
```

**Stack:** Pop 1

**Position:** If the value on top of the stack is `true`, jumps to the given label, else next.

## `call` (Call)
**Buffer Value:** `0x04`

**Summary:** Calls the given function. Pops and pushes values onto the stack as given by the function signature.

**Layout and Parameter(s):**
```
call {0}:{1}({2}) -> {3};
```
- `{0}` = Package identifier (*optional*)
- `{1}` = Function identifier
- `{2}` = Parameter types (*optional*)
- `{3}` = Return type

**Example(s):**
```
// Stack: Pop 4, Push 1
call another_package.inner:advanced_function(bl, int, fl, str) -> str;

// Stack: Pop 2, Push 1
call std.opr.bit:shl(int, int) -> int;

// Stack: Pop 1
call std.sio:writeln(str) -> vd;

// Stack: Push 1
call std.env:date() -> str;

// Stack: N/A
call my_simple_function() -> vd;
```

**Stack:** Pops and pushes as given by the function signature.

**Position:** Calls the given function.

## `ret` (Return)
**Buffer Value:** `0x05`

**Summary:** Returns from the current function.

**Layout and Parameter(s):**
```
ret;
```

**Example(s):**
```
ret;
```

**Stack:** Pop 1 if the return type of the function is not `vd`.

**Position:** Returns from the current function. Exits the program if the current function is the entry-point.