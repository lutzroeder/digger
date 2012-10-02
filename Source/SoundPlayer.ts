module Digger
{
    export class SoundPlayer
    {
        private _soundTable: {} = {};
        private _audioElement: HTMLAudioElement;
        private _audioContext: AudioContext;

        constructor()
        {
            window.AudioContext = window.AudioContext || (window as any).webkitAudioContext;
            if (window.AudioContext)
            {
                this._audioContext = new AudioContext();
            }
            else
            {
                this._audioElement = <HTMLAudioElement> document.createElement('audio');
            }
        }

        public load(name: string, data: string)
        {
            var soundTable = this._soundTable;
            if (this._audioContext)
            {
                var decodedData = window.atob(data);
                var length = decodedData.length;
                var buffer = new ArrayBuffer(length);
                var array = new Uint8Array(buffer);
                for(var i = 0; i < length; i++)
                {
                    array[i] = decodedData.charCodeAt(i);
                }
                this._audioContext.decodeAudioData(buffer, function(audio) {
                    soundTable[name] = audio; 
                }, function() { 
                    console.error("decodeAudioData"); 
                });
            }
            else
            {
                soundTable[name] = "data:audio/wav;base64," + data;
            }
        }

        public play(name: string) : boolean
        {
            var sound = this._soundTable[name];
            if (sound)
            {
                if (this._audioContext)
                {
                    var audioBufferSource = this._audioContext.createBufferSource();
                    audioBufferSource.buffer = sound;
                    audioBufferSource.connect(this._audioContext.destination);
                    audioBufferSource.start(0);
                }
                else
                {
                    this._audioElement.src = sound;
                    this._audioElement.play();
                }
                return true;
            }
            return false;
        }
    }
}