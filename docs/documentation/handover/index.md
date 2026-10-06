
# Handover Documentation

This documentation section was created specifically to aid the knowledge transfer in the handover of the project to the next team.

### Index

<script setup>
  /** 
   * FYI for future developers, this is Vue (v3+) syntax
  */
  import { data } from "../../.vitepress/build-time-data-loaders/handover.data.ts"
</script>

<ul>
  <li v-for="d of data" style="list-style-type:square">
    <a :href="d.url">{{d.title}}</a>
  </li>
</ul>