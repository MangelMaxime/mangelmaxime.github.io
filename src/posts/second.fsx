(***
title: This is post written using F# literate
layout: layouts/base.njk
***)

// This is a code block
let value = "Hello World"

(***
# This a documentation comment

Lorem ipsum dolor sit amet, consectetur adipiscing elit.
Donec euismod, nisl eget consectetur consectetur, nisl nunc egestas nisi, eu consectetur nunc nisi euismod nunc.

***)

// Move code block below

let add (a : int) (b : int) = a + b
(*** show ***)

(**

// This Nunjucks instructions are not processed

{% set lastModifiedDate = page.inputPath | lastModifiedDate | formatDate %}

<i>Last updated on <strong>{{ lastModifiedDate }}</strong></i>

**)
