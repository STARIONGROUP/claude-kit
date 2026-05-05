# Skills

Claude Code skills are markdown files that define reusable prompt templates invokable via the `Skill` tool.

## Structure

Each skill lives in its own subdirectory containing:
- `SKILL.md` — the skill definition (prompt template with YAML frontmatter)
- `<skill-name>.cs` — the executable .NET script the skill invokes

The directory name becomes the skill name (e.g., `skills/my-skill/` → `/my-skill`).

## Convention

```
skills/
├── README.md
└── <skill-name>/
    ├── SKILL.md
    └── <skill-name>.cs
```

## Anatomy of a Skill File

Skill `.md` files require YAML frontmatter with `name` and `description` fields, followed by the prompt body:

The `SKILL.md` file requires YAML frontmatter with `name` and `description` fields:

```markdown
---
name: my-skill
description: One-line description of what this skill does
---

# Skill Title

Brief description of what this skill does.

## Instructions

Detailed prompt instructions for Claude to follow when this skill is invoked.
```

The `name` field is the skill identifier and the `description` field helps Claude Code decide when to auto-invoke the skill.

## References

- [Claude Code Skills Documentation](https://code.claude.com/docs/en/skills)
