export type ThemeMode = 'light' | 'dark';

export type CssVarName = `--app-${string}`;

export type Primitive = string | number | boolean | null | undefined;

export type CssVars = Readonly<Record<CssVarName, string>>;
