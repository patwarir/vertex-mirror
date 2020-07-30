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
- [`rot`](#rot-stack-rotate)
- [`flp`](#flp-stack-flip)
- [`clr`](#clr-stack-clear)
- [`ld.lt`](#ldlt-load-literal)
- [`ld.gl`](#ldgl-load-global)
- [`ld.pr`](#ldpr-load-parameter)
- [`ld.lc`](#ldlc-load-local)
- [`st.lc`](#stlc-set-local)

## `nop` (No Operation)
**Buffer Value:** `0x0`

**Summary:** Does nothing.

**Layout and Parameters:**
```
nop;
```

**Stack:** N/A

**Position:** Next

## `jmp.a` (Jump Always)
**Buffer Value:** `0x1`

**Summary:** Jumps to the given label always.

**Layout and Parameters:**
```
jmp.a {0};
```
- `{0}` = Label identifier

**Stack:** N/A

**Position:** Jumps to the given label.

## `jmp.f` (Jump False)
**Buffer Value:** `0x2`

**Summary:** Jumps to the given label when the value on top of the stack is `false`.

**Layout and Parameters:**
```
jmp.f {0};
```
- `{0}` = Label identifier

**Stack:** Pop 1

**Position:** If the value on top of the stack is `false`, jumps to the given label, else next.

## `jmp.t` (Jump True)
**Buffer Value:** `0x3`

**Summary:** Jumps to the given label when the value on top of the stack is `true`.

**Layout and Parameters:**
```
jmp.t {0};
```
- `{0}` = Label identifier

**Stack:** Pop 1

**Position:** If the value on top of the stack is `true`, jumps to the given label, else next.