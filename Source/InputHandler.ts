module Digger
{
    export class InputHandler
    {
        private _canvas: HTMLCanvasElement;
        private _application: Application;
        private _touchPosition: Position;
        private _mouseDownHandler: (e: MouseEvent) => void;
        private _touchStartHandler: (e: TouchEvent) => void;
        private _touchEndHandler: (e: TouchEvent) => void;
        private _touchMoveHandler: (e: TouchEvent) => void;
        private _keyDownHandler: (e: KeyboardEvent) => void;
        private _keyUpHandler: (e: KeyboardEvent) => void;

        constructor(canvas: HTMLCanvasElement, application: Application)
        {
            this._canvas = canvas;
            this._application = application;

            this._mouseDownHandler = (e: MouseEvent) => { this.mouseDown(e); };
            this._touchStartHandler = (e: TouchEvent) => { this.touchStart(e); };
            this._touchEndHandler = (e: TouchEvent) => { this.touchEnd(e); };
            this._touchMoveHandler = (e: TouchEvent) => { this.touchMove(e); };
            this._keyDownHandler = (e: KeyboardEvent) => { this.keyDown(e); };
            this._keyUpHandler = (e: KeyboardEvent) => { this.keyUp(e); };

            this._canvas.addEventListener("touchstart", this._touchStartHandler, false);
            this._canvas.addEventListener("touchmove", this._touchMoveHandler, false);
            this._canvas.addEventListener("touchend", this._touchEndHandler, false);
            this._canvas.addEventListener("mousedown", this._mouseDownHandler, false);
            
            document.addEventListener("keydown", this._keyDownHandler, false);
            document.addEventListener("keyup", this._keyUpHandler, false);
        }

        private keyDown(e: KeyboardEvent)
        {
            if (!e.ctrlKey && !e.altKey && !e.altKey && !e.metaKey)
            {
                switch (e.keyCode)
                {
                    case 37: // left
                        this.stopEvent(e);
                        this._application.addKey(Key.left);
                        break;
                    case 39: // right
                        this.stopEvent(e);
                        this._application.addKey(Key.right);
                        break;
                    case 38: // up
                        this.stopEvent(e);
                        this._application.addKey(Key.up);
                        break;
                    case 40: // down
                        this.stopEvent(e);
                        this._application.addKey(Key.down);
                        break;
                    case 27: // escape
                        this.stopEvent(e);
                        this._application.addKey(Key.reset);
                        break;
                    case 8: // backspace
                    case 36: // delete
                        this.stopEvent(e);
                        this._application.nextLevel();
                        break;
                    default:
                        if (!this._application.isPlayerAlive())
                        {
                            this.stopEvent(e);
                            this._application.addKey(Key.reset); 
                        }
                        break;
                }
            }
        }

        private keyUp(e: KeyboardEvent)
        {
            switch (e.keyCode)
            {
                case 37: 
                    this._application.removeKey(Key.left);
                    break;
                case 39:
                    this._application.removeKey(Key.right);
                    break;
                case 38:
                    this._application.removeKey(Key.up);
                    break;
                case 40:
                    this._application.removeKey(Key.down);
                    break;
            }
        }

        private mouseDown(e: MouseEvent) 
        {
            e.preventDefault(); 
            this._canvas.focus();
        }

        private touchStart(e: TouchEvent)
        {
            e.preventDefault();
            if (e.touches.length > 3) // 4 finger touch = jump to next level
            {
                this._application.nextLevel();
            }
            else if ((e.touches.length > 2) || (!this._application.isPlayerAlive())) // 3 finger touch = restart current level
            {
                this._application.addKey(Key.reset);
            }
            else
            {
                for (var i = 0; i < e.touches.length; i++)
                {
                    this._touchPosition = new Position(e.touches[i].pageX, e.touches[i].pageY);
                }
            }
        }

        private touchMove(e: TouchEvent)
        {
            e.preventDefault();
            for (var i = 0; i < e.touches.length; i++)
            {
                if (this._touchPosition !== null)
                {
                    var x: number = e.touches[i].pageX;
                    var y: number = e.touches[i].pageY;
                    var direction: Key = null;
                    if ((this._touchPosition.x - x) > 20)
                    {
                        direction = Key.left;
                    }
                    else if ((this._touchPosition.x - x) < -20)
                    {
                        direction = Key.right;
                    }
                    else if ((this._touchPosition.y - y) > 20)
                    {
                        direction = Key.up;
                    }
                    else if ((this._touchPosition.y - y) < -20)
                    {
                        direction = Key.down;
                    }
                    if (direction !== null)
                    {
                        this._touchPosition = new Position(x, y);           
                        for (var i: number = Key.left; i <= Key.down; i++)
                        {
                            if (direction == i)
                            {
                                this._application.addKey(i);
                            }
                            else
                            { 
                                this._application.removeKey(i);
                            }
                        }
                    }
                }
            }
        }

        private touchEnd(e: TouchEvent)
        {
            e.preventDefault();
            this._touchPosition = null;
            this._application.removeKey(Key.left);
            this._application.removeKey(Key.right);
            this._application.removeKey(Key.up);
            this._application.removeKey(Key.down);
        }

        private stopEvent(e: Event)
        {
            e.preventDefault();
            e.stopPropagation();
        }
    }
}