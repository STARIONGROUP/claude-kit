# Slash Commands

Claude Code slash commands are markdown files that define custom `/command` shortcuts.

> **Note:** In current versions of Claude Code, skills and slash commands share a unified mechanism. Skill `.md` files with YAML frontmatter can also be invoked as slash commands. See `skills/README.md` for details.

## Structure

Each command is a `.md` file. The filename becomes the command name (e.g., `build.md` → `/build`).
Subdirectories create namespaced commands (e.g., `dotnet/build.md` → `/dotnet:build`).

## Convention

```
commands/
├── README.md
├── <command-name>.md
└── <namespace>/
    └── <command-name>.md
```

## Anatomy of a Command File

```markdown
# Command Title

$ARGUMENTS

Detailed instructions or prompt that Claude executes when the command is invoked.
Use `$ARGUMENTS` as a placeholder for any text the user passes after the command name.
```

## References

- [Claude Code Slash Commands Documentation](https://code.claude.com/docs/en/slash-commands)
