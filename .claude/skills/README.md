# Skills

Claude Code skills are markdown files (`SKILL.md`) that define reusable prompt templates and the scripts they invoke. When this folder lives at `.claude/skills/` (project) or `~/.claude/skills/` (user), Claude Code auto-discovers each subdirectory and registers it as a `/<folder-name>` slash command.

## Layout

```
.claude/skills/
├── README.md
└── <skill-name>/
    ├── SKILL.md
    └── <skill-name>.cs
```

The folder name becomes the slash command. The `name` field in `SKILL.md` frontmatter should match the folder name.

## Anatomy of a SKILL.md

```markdown
---
name: my-skill
description: One-line description of what this skill does and when to use it
---

# Skill Title

## Instructions

When the user asks to do X, run:

```bash
dotnet run "${CLAUDE_SKILL_DIR}/my-skill.cs" <args>
```
```

`${CLAUDE_SKILL_DIR}` is injected automatically by Claude Code and resolves to the skill's own directory — use it whenever a SKILL.md needs to reference a bundled script or asset, so the skill keeps working when copied to a different `.claude/skills/` location.

## Sharing

Each skill folder is self-contained. To share:

```bash
# To another project
cp -r .claude/skills/<name> /path/to/other-repo/.claude/skills/<name>

# To make it available in every project on this machine
cp -r .claude/skills/<name> ~/.claude/skills/<name>
```

## References

- [Claude Code Skills Documentation](https://code.claude.com/docs/en/skills)
