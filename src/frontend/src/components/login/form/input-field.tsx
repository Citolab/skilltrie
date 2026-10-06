/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { useState } from "react";

interface InputFieldProps {
  type: string;
  name: string;
  value: string;
  placeholder: string;
  className?: string;
  onChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
}

export default function InputField({type, name, value, placeholder, className, onChange}: InputFieldProps) {
  /** Whether the input field is currently selected or not */
  const [touchedState, setTouchedState] = useState<boolean>(false);
  const baseStyle = touchedState ? (
      "border border-blue-500 focus-within:ring-blue-500"
    ) : (
      "border-gray-300 dark:border-gray-600"
    );
  return (
    <div className="w-full flex justify-center">
      <div
        className={`relative w-[80%] sm:w-[70%] h-12 border-2 rounded-lg overflow-hidden bg-white dark:bg-white transition-all focus-within:ring-2 ${className} ${baseStyle}`}
      >
        <input
          type={type}
          id={name}
          name={name}
          value={value}
          onChange={onChange}
          onFocus={() => setTouchedState(true)}
          onBlur={() => setTouchedState(false)}
          placeholder={placeholder}
          className="w-full h-full px-4 text-black bg-transparent rounded-lg focus:outline-none text-center placeholder-black text-base"
        />
      </div>
    </div>
  );
}