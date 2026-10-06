---
# https://vitepress.dev/reference/default-theme-home-page
layout: home

hero:
    name: "SkillTrie Documentation"
    tagline: Documentation for developers & CITO
    actions:
        - theme: brand
          text: Handover Documentation
          link: /handover
        - theme: alt
          text: Auto Documentation
          link: /auto-docs

features:
    - title: Handover
      details: "Information relevant to the transfer of the project."
      link: "/handover"
      linkText: "Visit"
    - title: Developer Docs
      details: Information about developer tools and best practices.
      link: "/developer-docs"
      linkText: Visit
    - title: Auto Docs
      details: Auto generated documentation for the backend, based on XML comments.
      link: "/auto-docs"
      linkText: Visit
    - title: Backend & Frontend Docs
      details: Handwritten documentation for the backend & frontend.
---

<h2 style="text-align: center">Our Team:</h2>

<script setup>
import { VPTeamMembers } from "vitepress/theme"

const svg = "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0idXRmLTgiPz4KPCEtLSBHZW5lcmF0b3I6IEFkb2JlIElsbHVzdHJhdG9yIDIyLjEuMCwgU1ZHIEV4cG9ydCBQbHVnLUluIC4gU1ZHIFZlcnNpb246IDYuMDAgQnVpbGQgMCkgIC0tPgo8c3ZnIHZlcnNpb249IjEuMSIgaWQ9IkxheWVyXzEiIHhtbG5zPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2ZyIgeG1sbnM6eGxpbms9Imh0dHA6Ly93d3cudzMub3JnLzE5OTkveGxpbmsiIHg9IjBweCIgeT0iMHB4IgoJIHZpZXdCb3g9IjAgMCA1MDAgNTAwIiBzdHlsZT0iZW5hYmxlLWJhY2tncm91bmQ6bmV3IDAgMCA1MDAgNTAwOyIgeG1sOnNwYWNlPSJwcmVzZXJ2ZSI+CjxzdHlsZSB0eXBlPSJ0ZXh0L2NzcyI+Cgkuc3Qwe2ZpbGw6IzIzMUYyMDt9Cgkuc3Qxe2ZpbGw6IzEzQUZGMDt9Cjwvc3R5bGU+CjxnPgoJPHBhdGggY2xhc3M9InN0MCIgZD0iTTc2LDM2OS4zbDE3LjEsNTFoLTkuNEw4MCw0MDlINjIuNGwtMy41LDExLjNoLTkuNGwxNy4xLTUxSDc2eiBNNjUsNDAwLjZoMTIuNGwtNi4zLTE5LjdMNjUsNDAwLjZ6Ii8+Cgk8cGF0aCBjbGFzcz0ic3QwIiBkPSJNMTAwLjMsNDIwLjN2LTUwLjhoMjEuNWM4LjgsMCwxNC4xLDUuNCwxNC4xLDExLjJ2MTAuOGMwLDQuMS0yLjgsOC03LjUsOS45bDguNiwxOC45aC05LjlsLTguMS0xNy44aC05LjQKCQl2MTcuOEgxMDAuM3ogTTEwOS43LDM3Ny45djE2LjJoMTEuNGMzLDAsNS41LTEuNyw1LjUtNC43di02LjdjMC0zLjEtMi42LTQuOC01LjktNC44SDEwOS43eiIvPgoJPHBhdGggY2xhc3M9InN0MCIgZD0iTTE1NC42LDQyMC4zdi00Mi41SDE0MnYtOC4zaDMzLjZ2OC4zaC0xMS42djQyLjVIMTU0LjZ6Ii8+Cgk8cGF0aCBjbGFzcz0ic3QwIiBkPSJNMTg2LjksNDA1Ljh2My4zYzAsNC41LDQuMiw3LjksOS42LDcuOWg2YzUuNiwwLDkuOS0yLjcsOS45LTcuNFY0MDZjMC03LjgtMjguMS05LjMtMjguMS0yMi43di00LjYKCQljMC01LjEsNC45LTkuNiwxMS40LTkuNmg2LjdjNy4zLDAsMTIuNyw0LjksMTIuNywxMC40djQuNmgtNC4ydi0zLjhjMC00LjItMy45LTcuNC04LjgtNy40aC01LjhjLTQuMywwLTcuOCwyLjgtNy44LDYuNXYzLjcKCQljMCwxMCwyOC4xLDExLjMsMjguMSwyMi4zdjQuOWMwLDYuNC01LjksMTAuNC0xMy44LDEwLjRoLTYuNWMtNy45LDAtMTMuNy01LjEtMTMuNy0xMXYtMy45SDE4Ni45eiIvPgoJPHBhdGggY2xhc3M9InN0MCIgZD0iTTIzNy43LDQyMC4zdi00Ny4xaC0xNC4ydi0zLjdoMzJ2My43aC0xMy42djQ3LjFIMjM3Ljd6Ii8+Cgk8cGF0aCBjbGFzcz0ic3QwIiBkPSJNMjc4LjIsMzY5LjFsMTcuOSw1MS4xaC00LjNsLTUuNy0xNi4zaC0yMC4zbC01LjYsMTYuM0gyNTZsMTgtNTEuMUgyNzguMnogTTI2Ny4xLDQwMC4yaDE3LjdsLTguOC0yNS42CgkJTDI2Ny4xLDQwMC4yeiIvPgoJPHBhdGggY2xhc3M9InN0MCIgZD0iTTMxMC45LDQyMC4zdi00Ny4xaC0xNC4ydi0zLjdoMzJ2My43aC0xMy42djQ3LjFIMzEwLjl6Ii8+Cgk8cGF0aCBjbGFzcz0ic3QwIiBkPSJNMzM4LjEsNDIwLjN2LTUwLjhoNC4ydjUwLjhIMzM4LjF6Ii8+Cgk8cGF0aCBjbGFzcz0ic3QwIiBkPSJNMzY4LjQsNDIwLjdjLTcsMC0xMi01LjEtMTItMTF2LTI5LjZjMC01LjksNS4xLTExLDEyLTExaDEwLjFjNi45LDAsMTEuOSw1LjEsMTEuOSwxMXYyOS42CgkJYzAsNS45LTUsMTEtMTEuOSwxMUgzNjguNHogTTM3OC4yLDQxNi45YzQuNSwwLDguMS0zLjQsOC4xLTcuOXYtMjguM2MwLTQuNS0zLjYtNy45LTguMS03LjloLTkuNWMtNC41LDAtOC4xLDMuNC04LjEsNy45VjQwOQoJCWMwLDQuNSwzLjYsNy45LDguMSw3LjlIMzc4LjJ6Ii8+Cgk8cGF0aCBjbGFzcz0ic3QwIiBkPSJNNDA0LjUsNDIwLjN2LTUwLjhoNC40bDI1LjUsNDMuNHYtNDMuNGg0LjJ2NTAuOGgtNC40bC0yNS41LTQzLjR2NDMuNEg0MDQuNXoiLz4KPC9nPgo8Zz4KCTxwYXRoIGNsYXNzPSJzdDEiIGQ9Ik05NS43LDI3Ni42bDI1LjEsNDMuM2wwLDBjNC45LDkuOCwxNS4xLDE2LjUsMjYuOCwxNi41bDAsMGwwLDBoMTY2LjNsLTM0LjUtNTkuOEg5NS43eiIvPgoJPHBhdGggY2xhc3M9InN0MSIgZD0iTTM5Mi4zLDI3Ni45YzAtNi0xLjgtMTEuNi00LjgtMTYuMkwyOTAuMSw5MS4yYy01LTkuNS0xNS0xNS45LTI2LjUtMTUuOWgtNTEuNWwxNTAuNSwyNjAuOGwyMy43LTQxLjEKCQlDMzkxLjEsMjg3LjEsMzkyLjMsMjgzLjcsMzkyLjMsMjc2Ljl6Ii8+Cgk8cG9seWdvbiBjbGFzcz0ic3QxIiBwb2ludHM9IjI1NC44LDIzNC4xIDE4Ny41LDExNy41IDEyMC4yLDIzNC4xIAkiLz4KPC9nPgo8L3N2Zz4=";

const members = [
    {
        avatar: "https://secure.gravatar.com/avatar/deefc7696dba4379634f9c305eec7ed6670aaea0a07e95825481c802dabe6895?s=1600&d=identicon",
        name: "Ralph van Pul",
        title: "Chairman, Developer",
        links: [
          { icon: "gitlab", link: "https://git.science.uu.nl/4474090" },
        ]
    },
    {
        avatar: "https://secure.gravatar.com/avatar/9c2f1de4ccbb463c4c0c69c72f9abbd874593d9d28d6ea60213ea24478f9a4ca?s=1600&d=identicon",
        name: "Stin Thielen",
        title: "Scrum master, Developer",
        links: [
          { icon: "gitlab", link: "https://git.science.uu.nl/2759454" },
        ]
    },
    {
        avatar: "https://secure.gravatar.com/avatar/22c748d1c6315c503dc6a6058a08a4bf285adc1214b586968d52e4faf012b245?s=1600&d=identicon",
        name: "Timo Dijkstra",
        title: "Product Owner, Developer",
        links: [
          { icon: "gitlab", link: "https://git.science.uu.nl/t.dijkstra1" },
        ]
    },
    {
        avatar: "https://secure.gravatar.com/avatar/6bd7de9a90fe8a9e9d2eea1da9bcd8506c3f14d9d456902286a699ddbdcbd76e?s=1600&d=identicon",
        name: "Edwin Roeleveld",
        title: "Developer",
        links: [
          { icon: "gitlab", link: "https://git.science.uu.nl/9562850" },
        ]
    },
    {
        avatar: "https://secure.gravatar.com/avatar/ac952f0fdfa14a229d1386ca54bfd4de89f14bc6402985969b22bdef8bf9d205?s=1600&d=identicon",
        name: "Emir Keskinkilinc",
        title: "Developer",
        links: [
          { icon: "gitlab", link: "https://git.science.uu.nl/2471949" },
        ]
    },
    {
        avatar: "https://secure.gravatar.com/avatar/acae2b2907b9191130f0e08b8a534ce407e147f46c58a67df33eef19184ac834?s=1600&d=identicon",
        name: "Joris Stovers",
        title: "Developer",
        links: [
          { icon: "gitlab", link: "https://git.science.uu.nl/9091998" },
        ]
    },
    {
        avatar: "https://secure.gravatar.com/avatar/1c94afc2ddc08f8feb9bc4f63a0284e39c43f3de26897832221fd8da8d1ff1d5?s=1600&d=identicon",
        name: "Matthijs zur Muhlen",
        title: "Developer",
        links: [
          { icon: "gitlab", link: "https://git.science.uu.nl/m.s.j.zurmuhlen" },
        ]
    },
    {
        avatar: "https://git.science.uu.nl/uploads/-/system/user/avatar/4560/avatar.png?width=800",
        name: "Bas Franken",
        title: "Developer",
        links: [
          { icon: "gitlab", link: "https://git.science.uu.nl/2769417" },
        ]
    },
    {
        avatar: "https://git.science.uu.nl/uploads/-/system/user/avatar/6234/avatar.png?width=800",
        name: "Paul Stapel",
        title: "Developer",
        links: [
          { icon: "github", link: "https://github.com/PaulStapel" },
        ]
    },
    {
        avatar: "https://secure.gravatar.com/avatar/f311407d8a76ba1750e2705c9c05f3ed56aa32ae53b3583fc743fbbf52d46d03?s=1600&d=identicon",
        name: "Armand Ayar",
        title: "Developer",
        links: [
          { icon: "gitlab", link: "https://git.science.uu.nl/6402704" },
        ]
    },
    {
        avatar: "https://cdna.artstation.com/p/users/avatars/013/451/722/large/5c0798b84e5073d953f022c016363683.jpg?1761729406",
        name: "Bente Schelvis",
        title: "Artist",
        links: [
            { 
                icon: { svg: atob(svg) }, 
                link: "https://www.artstation.com/bschelvis" 
            },
        ]
    },
    {
        avatar: "https://cdnb.artstation.com/p/users/avatars/013/431/703/large/23dad09cd9f37e24763b091ebf2ef3fc.jpg?1757579750",
        name: "Alex van der Meer",
        title: "Artist",
        links: [
            { 
                icon: { svg: atob(svg) }, 
                link: "https://www.artstation.com/alex_ares17" 
            },
        ]
    }
]
</script>

<VPTeamMembers size="small" :members />
