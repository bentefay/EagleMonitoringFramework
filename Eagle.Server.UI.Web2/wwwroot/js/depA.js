var DepA = (function () {
    function DepA() {
    }
    DepA.prototype.greet = function () {
        return "Hello!";
    };
    return DepA;
})();
module.exports = DepA;
