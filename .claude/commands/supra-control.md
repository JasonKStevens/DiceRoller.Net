---
description: Coordinates the implementation of a change using the various sub-agents
---

# Supra-Control

This command co-ordinates the use of the various subagents in order to implement work.

Output a message to the user saying 'CONTROL command started'

## Instructions
1. Run `git ls-files` to understand the codebase structure and file organization
2. Read the README.md to understand the project purpose, setup instructions, and key information
3. Assume all work is going to be multi-phase and use sequential thinking to:
 - Plan optimal agent delegation sequence
 - Identify dependencies between tasks
 - Anticipate potential bottlenecks
 - Design parallel execution opportunities
 - Track workflow state and progress

 ## Workflow Patterns

### Continuous Improvement Loop
After each significant workflow:
```
Delegate to retrospective: "Review the changes made in [workflow type] completion:
- Workflow duration: [actual vs estimated]
- Issues encountered: [list]
- Agent performance: [notes]
- Final outcome: [success/partial/failed]"
```
Use TodoWrite to output updated Todo information to keep the user up to date

Incorporate retrospective recommendations into future workflows.

### Bug Fixing - Non-UI
For changes that do not involve the User Interface:
```
1. Delegate to analyzer: "Analyse this bug report [bug description] to determine why it is happening"
2. Delegate to architect: "Design a solution to [bug description] based on the feedback from the analyzer [analyzer response]"
3. Delegate to implementer: "Implement a fix for [bug description] reported by user based upon the feedback from the analyzer [analyzer reponse] and the architect [architect response]"
   - **CRITICAL**: Ensure implementer follows progressive debugging (Client → Network → Server)
4. Monitor implementer progress and escalate if blocked
5. If the implementer indicates failure, return to step 1, but pass feedback
6. Delegate to the reviewer: "Review the code changes made by the implementer"
7. If the reviewer is unhappy with the changes, then return to step 3 with its feedback
8. Delegate to the retrospective agent: "Hold a retrospective on the work that was done to resolve this bug"
9. Delegate to the documenter: "Update the documentation based off the changes made in this process, update an todo's necessary and update any agents with critial/important learnings"
```

### Bug Fixing - UI
For changes involving a web UI:
```
1. Delegate to analyzer: "Analyse this bug report [bug description] to determine why it is happening"
2. Delegate to architect: "Design a solution to [bug description] based on the feedback from the analyzer [analyzer response]"
3. Delegate to web-ui-builder: "Implement a fix for [bug description] reported by user based upon the feedback from the analyzer [analyzer reponse] and the architect [architect response]"
   - **CRITICAL**: Ensure implementer follows progressive debugging (Client → Network → Server)
4. Monitor implementer progress and escalate if blocked
5. If the implementer indicates failure, return to step 1, but pass feedback
6. Delegate to the reviewer: "Review the code changes made by the implementer"
7. If the reviewer is unhappy with the changes, then return to step 3 with its feedback
8. Delegate to the retrospective agent: "Hold a retrospective on the work that was done to resolve this bug"
9. Delegate to the documenter: "Update the documentation based off the changes made in this process, update an todo's necessary and update any agents with critial/important learnings"
```


### Feature Development
```
1. Delegate to analyzer: "Analyze [area] for [feature requirements]"
2. Delegate to architect: "Design solution for [analyzed requirements]"
3. Delegate to implementer: "Implement this based upon the feedback from the analyzer [analyzer reponse] and the architect [architect response]"
   - **CRITICAL**: Ensure implementer follows progressive debugging (Client → Network → Server)
4. Monitor implementer progress and escalate if blocked
5. If the implementer indicates failure, return to step 1, but pass feedback
6. Delegate to the reviewer: "Review the code changes made by the implementer"
7. If the reviewer is unhappy with the changes, then return to step 3 with its feedback
8. Delegate to ui-validator: "Run E2E tests for [feature]"
   - If E2E tests fail:
     a. Analyze failure type (functionality vs missing implementation)
     b. Route to appropriate agent:
        - implementer: For broken existing functionality
        - implementer: For missing/incomplete functionality
     c. Include all debug info (screenshots, logs, network errors)
     d. Return to step 5 after fixes
9. Delegate to the retrospective agent: "Hold a retrospective on the work that was done to resolve this bug"
10. Delegate to the documenter: "Update the documentation based off the changes made in this process, update an todo's necessary and update any agents with critial/important learnings"
```

## E2E Test Quality Gate

### Mandatory for All UI Changes
- **No work is complete** until E2E tests pass
- E2E tests run automatically after reviewer approval
- Failures trigger automatic feedback loop to appropriate agent
- All debug information automatically collected and provided

### E2E Failure Routing
```
E2E Test Failure → Analyze Failure Type
├─ Broken Functionality → implementer
├─ Missing Implementation → implementer
├─ UI Regression → web-ui-builder
└─ Integration Issue → architect → implementer
```

### Debug Information Provided
- Screenshots (full page)
- Console logs (all messages)
- Network errors (failed requests)
- Page source (HTML snapshot)
- Test execution timeline
- Specific assertion failures

## Key Principles
- Always enforce behavioral testing over implementation testing
- Maintain clear task boundaries between agents
- Track progress systematically
- Ensure each phase completes before proceeding
- **E2E tests are mandatory quality gate** - no exceptions
- **Ask users for business logic decisions** rather than making assumptions
- **For form/validation bugs**: Always follow systematic debugging methodology (see `/docs/development/DEBUGGING_METHODOLOGY_LESSONS_2025-08-08.md`)

## Critical Honesty Requirements
- **VERY IMPORTANT**: Tell the truth, do not lie or make things up. Only report what is real, verified and factual
- **NEVER**: Generate code, data or explanations that give the impression something works if it does not or if you have not proven it does
- **BRUTAL HONESTY BEFORE ALL ELSE**: If a solution fails, admit it. If something is unclear, say so
- **NEVER**: Produce code, solutions or documentation that might mislead anyone about what is and is not working, possible or integrated
- **IF UNABLE TO FULFILL A TASK**: Communicate the facts and reasons clearly:
  - If an API doesn't exist, say so
  - If a system cannot be accessed, report it
  - If something is not possible, explain why
  - If clarification is needed, request it
- **ALWAYS**: Closely inspect results from all subagents to ensure they are telling the truth
- **VALIDATION**: Verify all claims made by subagents before accepting them as fact

## Business Logic Decision Guidelines
When encountering business/system logic decisions that affect user experience or system behavior, **ALWAYS ask the user** for their preference rather than making assumptions.

### Examples Requiring User Input:
- **Date/Time Formats**: "MM/dd/yyyy" vs "yyyy-MM-dd" vs "dd/MM/yyyy"
- **Number Formatting**: Leading zeros (D2) vs no padding
- **File Naming Conventions**: Underscore vs dash separators, casing preferences
- **Directory Structures**: Folder organization and naming patterns
- **UI Layout Choices**: Form layouts, button placement, navigation patterns
- **Data Display Options**: Sorting defaults, pagination sizes, column visibility

### Decision Protocol:
1. **Identify the Decision**: Recognize when a choice affects business logic
2. **Present Options**: Clearly explain the alternatives and their implications
3. **Ask for Preference**: "Would you prefer [Option A] or [Option B]? This affects [specific impact]"
4. **Document Choice**: Record the decision for future consistency
5. **Implement Consistently**: Apply the choice across all relevant areas

## Success Criteria
- All changes have behavioral tests
- Architecture remains consistent
- Documentation stays current
- Zero regression in functionality
- Continuous improvement through retrospectives

## Workflow Tracking
For retrospective analysis, track:
- Start/end times for each phase
- Issues encountered
- Review cycles needed
- Final quality assessment