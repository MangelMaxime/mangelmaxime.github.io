---
title: This is my first post.
description: This is a post on My Blog about agile frameworks.
layout: layouts/base.njk
---

# This is my first post.

This is a post on My Blog about agile frameworks.

Test: {{ pageBodyClass }}

{{ '🚨' | favIconFromEmoji | safe }}

```fs
type Test =
    {
        Name: string
        Age: int
    }

    static member Create name age =
        {
            Name = name
            Age = age
        }

let add (a: int) (b: int) = a + b
```