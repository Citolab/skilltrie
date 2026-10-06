
# Backend Documentation

Here you will find **handwritten** documentation pertaining to the backend.

### Index

<script setup>
  /** 
   * FYI for future developers, this is Vue (v3+) syntax
  */
  import { data } from "../../../.vitepress/build-time-data-loaders/backend.data.ts"
</script>

<ul>
  <li v-for="d of data" style="list-style-type:square">
    <a :href="d.url">{{d.title}}</a>
  </li>
</ul>