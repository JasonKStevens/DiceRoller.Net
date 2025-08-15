# Knowledge Entry: JavaScript File Size Chunked Upload Patterns

## Metadata
- **Created**: 2025-01-08
- **Expires**: 2025-04-08 (90 days TTL)
- **Source**: Web Research
- **Confidence**: High
- **Version**: 1.0

## Summary
Best practices for implementing JavaScript file size-based upload strategies with chunked uploads for large files (100MB+ threshold) and progress tracking.

## Details

### File Size Thresholds
- **100MB threshold**: Files ≥100MB should use chunked upload
- **Chunk sizes**: 5-10MB chunks recommended for production
- **Testing sizes**: 1MB chunks for development/testing

### Implementation Strategy

#### Simple Upload (< 100MB)
- Single HTTP request
- Native browser progress tracking
- Minimal complexity
- Risk of timeout/memory issues for large files

#### Chunked Upload (≥ 100MB)
- Split files into smaller chunks (5-10MB)
- Sequential chunk processing
- Enhanced progress tracking
- Resumable upload capability
- Better error handling

### Progress Tracking Best Practices
```javascript
// Total progress calculation
var totalPercentComplete = Math.round(
    (chunkCounter - 1) / numberOfChunks * 100 + 
    percentComplete / numberOfChunks
);
```

### Key Implementation Patterns
1. **File slicing**: `file.slice(start, start + chunkSize)`
2. **Sequential processing**: Wait for each chunk before sending next
3. **Retry logic**: 3 retries with exponential backoff
4. **Memory management**: Avoid large Base64 strings
5. **Resume capability**: Cache progress in localStorage

### Error Handling
- Retry failed chunks (3 attempts)
- Exponential backoff for retries
- Server-side cleanup of failed uploads
- User feedback for errors

### Security Considerations
- File signature validation
- MIME type validation
- Size validation before upload
- Server-side chunk validation

## Examples

### File Size Detection
```javascript
const CHUNK_THRESHOLD = 100 * 1024 * 1024; // 100MB
if (file.size >= CHUNK_THRESHOLD) {
    await this.uploadFileChunked(file);
} else {
    await this.uploadFileSimple(file);
}
```

### Chunk Processing
```javascript
const chunkSize = 5 * 1024 * 1024; // 5MB
let start = 0;
while (start < file.size) {
    const chunk = file.slice(start, start + chunkSize);
    await uploadChunk(chunk, start / chunkSize);
    start += chunkSize;
}
```

## References
- Transloadit: Optimizing file uploads with chunking
- Medium: Chunked uploads with Express.js and React
- DEV Community: Seamless large file uploads with JavaScript
- Stack Overflow: JS chunked file upload patterns

## Update History
- 2025-01-08: Initial research and documentation