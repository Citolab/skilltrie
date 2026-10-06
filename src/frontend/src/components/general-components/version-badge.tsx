/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

declare const __APP_VERSION__: string;

const VersionBadge = ({ className }: { className?: string }) => (
  <span className={`
    text-xs font-ui font-medium
    text-text-muted
    bg-transparent
    border border-borderDisabled
    px-2 py-0.5 rounded-full
    ${className ?? ''}
  `}>
    v{ __APP_VERSION__ ?? '-dev'}
  </span>
);

export default VersionBadge;