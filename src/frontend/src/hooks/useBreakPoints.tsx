/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import useMediaQuery from '@mui/material/useMediaQuery';

export function useBreakPoints() {
  return {
    isMobile: useMediaQuery('(max-width: 767px)'),
    isLandscape: useMediaQuery('(max-height: 430px) and (max-width: 932px)'),
    isTablet: useMediaQuery('(max-width: 1024px)'),
    isDesktop: useMediaQuery('(min-width: 1025px)'),
  };
}