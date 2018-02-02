/**
 * Navbar spacer class
 * @param {PSVNavBar} navbar
 * @param {int} [weight=5]
 * @constructor
 * @extends module:components.PSVComponent
 * @memberof module:components
 */
function PSVNavBarSpacer(navbar, weight) {
  PSVComponent.call(this, navbar);

  /**
   * @member {int}
   * @readonly
   */
  this.weight = weight || 5;

  this.create();

  this.container.classList.add('psv-spacer--weight-' + this.weight);
}

PSVNavBarSpacer.prototype = Object.create(PSVComponent.prototype);
PSVNavBarSpacer.prototype.constructor = PSVNavBarSpacer;

PSVNavBarSpacer.className = 'psv-spacer';
