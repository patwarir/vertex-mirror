# Operation Codes

## `nop` (No Operation)
**Summary:** Does nothing.

**Layout and Parameters:**
```
nop;
```

**Stack:** N/A

**Position:** Next

## `jmp.a` (Jump Always)
**Summary:** Jumps to the given label always.

**Layout and Parameters:**
```
jmp.a {0};
```
- `{0}` = Label identifier

**Stack:** N/A

**Position:** Jumps to the given label.

## `jmp.f` (Jump False)
**Summary:** Jumps to the given label when the value on top of the stack is `false`.

**Layout and Parameters:**
```
jmp.f {0};
```
- `{0}` = Label identifier

**Stack:** Pop 1

**Position:** If the value on top of the stack is `false`, jumps to the given label, else next.