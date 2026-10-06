
# Developer Documentation

In this section you, as a developer, will find information about specific tooling and practices that are specific to this project.

### Index

<script setup>
  /** 
   * FYI for future developers, this is Vue (v3+) syntax
  */
  import { data } from "../../.vitepress/build-time-data-loaders/dev.data.ts"
</script>

<ul>
  <li v-for="d of data" style="list-style-type:square">
    <a :href="d.url">{{d.title}}</a>
  </li>
</ul>