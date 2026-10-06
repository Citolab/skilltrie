/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { useEffect, useId, useRef, useState, type ChangeEvent, type ReactNode } from "react";
import CheckboxMui from "@mui/material/Checkbox";
import FormControlLabel from "@mui/material/FormControlLabel";
import { useFormContext } from "../../general-components/form";

interface CheckboxProps {
  checked: boolean;
  onChange: (e: ChangeEvent<HTMLInputElement>) => void;
  label?: ReactNode;
  required?: boolean;
}

export default function Checkbox({
  checked = false,
  onChange,
  label,
  required = false
}: CheckboxProps) {
  const [error, setError] = useState<boolean>(false);
  const form = useFormContext();
  const inputRef = useRef<HTMLInputElement | null>(null);
  const id = useId();

  useEffect(() => {
    form?.registerField(id, { validate:() => !required || checked, focus:() => inputRef.current?.focus() });
  }, [checked]);

  useEffect(() => {
    if (form?.submitCount && form.submitCount > 0)
      setError(required && !checked);
  }, [form?.submitCount]);

  return (
    <FormControlLabel
      label={label}
      sx={{ color: "var(--color-text-dark)" }}
      control={<CheckboxMui
        name="checkbox"
        checked={checked}
        onChange={(event) => {
          setError(false);
          onChange(event);
        }}
        slotProps={{
          input: {
            ref: inputRef
          }
        }}
        sx={{
          color: error ? "var(--color-error)" : "var(--color-text-dark)",
          "&.Mui-checked": { color: "var(--color-primary)" },
        }}
      />}
    />
  );
}
