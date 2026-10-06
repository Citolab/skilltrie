/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { useEffect, useState } from "react";

const TYPE_SPEED = 200;
const DELETE_SPEED = 150;
const PAUSE_DURATION = 3000;

const words = ["Fun", "Easy", "Trie", "Fast", "Next", "Bold"];
const longestWord = words.reduce((a, b) => (a.length >= b.length ? a : b));

export default function HeroTitle() {
  const [text, setText] = useState("");
  const [wordIndex, setWordIndex] = useState(0);
  const [isDeleting, setIsDeleting] = useState(false);
  const [isPausing, setIsPausing] = useState(false);

  const currentWord = words[wordIndex];   
  const speed = isDeleting ? DELETE_SPEED : TYPE_SPEED;
  
  useEffect(() => {
    if (isPausing) {
      const timer = setTimeout(() => {
        setIsPausing(false); 
        setIsDeleting(true);
      }, PAUSE_DURATION);
      return () => clearTimeout(timer);  
    }

    const timer = setTimeout(() => {
      if (!isDeleting) {
        const next = currentWord.substring(0, text.length + 1);
        setText(next);
        if (next === currentWord) {
          setIsPausing(true);  // pause branch will trigger when this branch is done
        }
      } else {
        const next = currentWord.substring(0, text.length - 1);
        setText(next);
        if (next === "") {
          setIsDeleting(false);
          setWordIndex((prev) => (prev + 1) % words.length);
        }
      }
    }, speed);

    return () => clearTimeout(timer);  
  }, [text, isDeleting, isPausing, wordIndex]);

  return (
    <h1
      className="
          titleText
          text-center
      "
      aria-label="Learn Subjects the Smart Way"
      >
      Learn Subjects
      <br />

      the{" "}

      <span className="relative inline-flex items-center">
          {/* Invisible width holder */}
          <span
          aria-hidden="true"
          className="
              invisible
          "
          >
          {longestWord}
          </span>

          <span
          aria-hidden="true"
          className="
              absolute
              inset-0
              flex
              justify-center
              text-secondary
              font-bold
          "
          >
          {text}
          </span>
      </span>
      {" "}Way
      </h1>
  );
}
