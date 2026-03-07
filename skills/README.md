# Skills

Claude Code skills are markdown files that define reusable prompt templates invokable via the `Skill` tool.

## Structure

Each skill is a `.md` file. The filename becomes the skill name (e.g., `my-skill.md` → `/my-skill`).

## Convention

```
skills/
├── README.md
└── <skill-name>.md
```

## Anatomy of a Skill File

```markdown
# Skill Title

Brief description of what this skill does.

## Instructions

Detailed prompt instructions for Claude to follow when this skill is invoked.
```

## References

- [Claude Code Skills Documentation](https://docs.anthropic.com/en/docs/claude-code/skills)
