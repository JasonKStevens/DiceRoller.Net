#!/usr/bin/env -S uv run --script
# /// script
# requires-python = ">=3.8"
# dependencies = [
#     "elevenlabs",
#     "python-dotenv",
# ]
# ///

import os
import sys
from pathlib import Path
from dotenv import load_dotenv
from datetime import datetime

def log_debug(msg):
    log_file = Path(os.getcwd()) / "logs" / "tts_simple.log"
    log_file.parent.mkdir(exist_ok=True)
    with open(log_file, 'a') as f:
        f.write(f"[{datetime.now().isoformat()}] {msg}\n")

def main():
    """
    Simple ElevenLabs TTS Script with debug logging
    """
    
    log_debug("Script started")
    
    # Load environment variables
    load_dotenv()
    
    # Get API key from environment
    api_key = os.getenv('ELEVENLABS_API_KEY')
    if not api_key:
        log_debug("No API key found")
        sys.exit(1)
    
    log_debug(f"API key found: {api_key[:10]}...")
    
    try:
        from elevenlabs.client import ElevenLabs
        from elevenlabs import play
        
        log_debug("ElevenLabs imported")
        
        # Initialize client
        elevenlabs = ElevenLabs(api_key=api_key)
        
        log_debug("Client initialized")
        
        # Get text from command line argument or use default
        if len(sys.argv) > 1:
            text = " ".join(sys.argv[1:])
        else:
            text = "Subagent Complete"
        
        log_debug(f"Text: {text}")
        
        try:
            # Generate and play audio directly
            log_debug("Calling API...")
            audio = elevenlabs.text_to_speech.convert(
                text=text,
                voice_id="1e9Gn3OQenGu4rjQ3Du1",  # User's preferred voice
                model_id="eleven_turbo_v2_5",
                output_format="mp3_44100_128",
            )
            
            log_debug("API call successful, playing audio...")
            play(audio)
            log_debug("Playback complete")
            
        except Exception as e:
            log_debug(f"Error in audio generation: {type(e).__name__}: {str(e)}")
            import traceback
            log_debug(traceback.format_exc())
        
    except Exception as e:
        log_debug(f"Error in main: {type(e).__name__}: {str(e)}")
        import traceback
        log_debug(traceback.format_exc())

if __name__ == "__main__":
    main()