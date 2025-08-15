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
import tempfile
import subprocess
from pathlib import Path
from datetime import datetime
from dotenv import load_dotenv

def log_debug(msg):
    log_file = Path(os.getcwd()) / "logs" / "tts_save_and_play.log"
    log_file.parent.mkdir(exist_ok=True)
    with open(log_file, 'a') as f:
        f.write(f"[{datetime.now().isoformat()}] {msg}\n")

def main():
    """
    ElevenLabs TTS Script that saves and plays using default Windows player
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
        
        # Create temp file
        temp_dir = Path(tempfile.gettempdir()) / "claude_tts"
        temp_dir.mkdir(exist_ok=True)
        temp_file = temp_dir / f"subagent_{os.getpid()}.mp3"
        
        try:
            # Generate audio
            log_debug("Calling API...")
            audio = elevenlabs.text_to_speech.convert(
                text=text,
                voice_id="1e9Gn3OQenGu4rjQ3Du1",  # User's preferred voice
                model_id="eleven_turbo_v2_5",
                output_format="mp3_44100_128",
            )
            
            log_debug("API call successful, saving audio...")
            
            # Save audio to file
            with open(temp_file, 'wb') as f:
                for chunk in audio:
                    f.write(chunk)
            
            log_debug(f"Audio saved to: {temp_file}")
            
            # Play using Windows default player
            log_debug("Starting Windows Media Player...")
            # Use start with /min to minimize window
            subprocess.run(['cmd', '/c', 'start', '/min', '', str(temp_file)], 
                         shell=False, check=False)
            
            log_debug("Playback initiated")
            
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