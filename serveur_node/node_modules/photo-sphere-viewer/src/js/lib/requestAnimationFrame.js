/**
 * requestAnimationFrame polyfill
 * {@link http://mattsnider.com/cross-browser-and-legacy-supported-requestframeanimation}
 * @license MIT
 */
(function(w) {
    "use strict";
    // most browsers have an implementation
    w.requestAnimationFrame = w.requestAnimationFrame ||
            w.mozRequestAnimationFrame || w.webkitRequestAnimationFrame ||
            w.msRequestAnimationFrame;
    w.cancelAnimationFrame = w.cancelAnimationFrame ||
            w.mozCancelAnimationFrame || w.webkitCancelAnimationFrame ||
            w.msCancelAnimationFrame;

    // polyfill, when necessary
    if (!w.requestAnimationFrame) {
        var aAnimQueue = [],
            aProcessing = [],
            iRequestId = 0,
            iIntervalId;

        // create a mock requestAnimationFrame function
        w.requestAnimationFrame = function(callback) {
            aAnimQueue.push([++iRequestId, callback]);

            if (!iIntervalId) {
                iIntervalId = setInterval(function() {
                    if (aAnimQueue.length) {
                        var time = +new Date();
                        // Process all of the currently outstanding frame
                        // requests, but none that get added during the
                        // processing.
                        // Swap the arrays so we don't have to create a new
                        // array every frame.
                        var temp = aProcessing;
                        aProcessing = aAnimQueue;
                        aAnimQueue = temp;
                        while (aProcessing.length) {
                            aProcessing.shift()[1](time);
                        }
                    } else {
                        // don't continue the interval, if unnecessary
                        clearInterval(iIntervalId);
                        iIntervalId = undefined;
                    }
                }, 1000 / 50);  // estimating support for 50 frames per second
            }

            return iRequestId;
        };

        // create a mock cancelAnimationFrame function
        w.cancelAnimationFrame = function(requestId) {
            // find the request ID and remove it
            var i, j;
            for (i = 0, j = aAnimQueue.length; i < j; i += 1) {
                if (aAnimQueue[i][0] === requestId) {
                    aAnimQueue.splice(i, 1);
                    return;
                }
            }

            // If it's not in the queue, it may be in the set we're currently
            // processing (if cancelAnimationFrame is called from within a
            // requestAnimationFrame callback).
            for (i = 0, j = aProcessing.length; i < j; i += 1) {
                if (aProcessing[i][0] === requestId) {
                    aProcessing.splice(i, 1);
                    return;
                }
            }
        };
    }
})(window);
