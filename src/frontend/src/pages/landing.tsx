/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { useNavigate } from "react-router-dom";
import { Link } from "react-router-dom";
import { motion } from "motion/react";
import Button from "../components/general-components/general-button";
import { Title } from "../components/general-components/title.tsx";
import HeroTitle from "../components/general-components/hero-title.tsx";
import backgroundSvg from "../assets/background.svg";
import backgroundPng from "../assets/background.png";
import train from "../assets/train.svg";

export default function LaunchPage() {
    const navigate = useNavigate();

    return (
        <div className="relative isolate flex flex-col min-h-screen pageBackground">
            {/* Hero scene + train share one "stage" sized exactly like `object-cover`
                (centered, always covering the viewport). Because the train lives inside
                this same scaled/cropped scene and is positioned by a percentage of the
                stage (i.e. in image coordinates), it stays glued to the rails at every
                screen resolution. The image is 1924.11 x 1089.06:
                  width  = max(100vw, (1924.11/1089.06) * 100vh) -> 176.68vh
                  height = max(100vh, (1089.06/1924.11) * 100vw) ->  56.60vw            */}
            <div
                className="absolute inset-0 -z-10 overflow-hidden"
                aria-hidden="true"
            >
                <div className="absolute left-1/2 top-1/2 h-[max(100vh,56.6vw)] w-[max(100vw,176.68vh)] -translate-x-1/2 -translate-y-1/2">
                    {/* Background: SVG with PNG fallback, fills the stage exactly */}
                    <picture>
                        <source srcSet={backgroundSvg} type="image/svg+xml" />
                        <img
                            src={backgroundPng}
                            alt=""
                            className="absolute inset-0 h-full w-full object-cover"
                        />
                    </picture>

                    {/* Train scrolling left-to-right along the tracks. The rails sit at
                        ~y686 of 1089 in the artwork; tweak `bottom-[…]` to fine-tune. Thanks AI for calculating */}
                    <motion.div
                        className="absolute bottom-[23%] aspect-[1701.5/114.23] w-[300%]"
                        initial={{ x: "-100%" }}
                        animate={{ x: "100%" }}
                        transition={{
                            duration: 40,
                            ease: "linear",
                            repeat: Infinity,
                        }}
                    >
                        <img
                            src={train}
                            alt=""
                            className="absolute inset-0 h-full w-full"
                        />
                    </motion.div>
                </div>
            </div>

            {/* Dark overlay: dims the hero image and the train into the background */}
            <div
                className="absolute inset-0 -z-10 bg-black/80"
                aria-hidden="true"
            ></div>

            {/* Header */}
            <header className="m-5 flex">
                <Title />
                <nav className="absolute top-0 right-0 px-6 py-4 flex flex-row max-[370px]:flex-col justify-center gap-2">
                    <Link
                        to="/login"
                        className="text-primary font-ui hover:underline self-center"
                    >
                        Log in
                    </Link>
                    <Button
                        type = "button"
                        variant = "primary"
                        size="small"
                        onClick={() => navigate("/register")}
                    >
                        Register
                    </Button>
                </nav>
            </header>

            {/* Body */}
            <main className="m-10 flex flex-col grow items-center justify-center gap-5">
                <img
                    src="/skilltrie_logo.svg"
                    alt="skilltrie logo"
                    className="w-25 h-25 rounded-full"
                />

                <HeroTitle/>

                <p className="paragraphText text-center max-w-120">
                    Build intuition through interactive exercises, visualizations, and real-world data. No jargon, just clarity.
                </p>

                <Button
                    type = "button"
                    variant = "primary"
                    size="medium"
                    onClick={() => navigate("/register")}
                >
                    Start Learning
                </Button>
            </main>

            {/* Footer */}
            <footer className="m-5 noteText text-center">
                A web-app built by Stastiftics and continued by SkillTrie
                <br/>
                © Utrecht University (ICS).
            </footer>
        </div>
    );
}
