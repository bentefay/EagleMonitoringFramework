== Modal.js

Some changes needed to be made to modal.js to make it compatible with being namespaced. In particular, 
there is some code to determine whether showing the modal will show or hide the scrollbar. This logic
currently assumes that the margin of the body tag is 0px (because this is set by bootstrap), but 
this will not be true when namespaced. Hence we need to take the margin of the body tag into account
when calculating the width of the body tag. 

The change is on line 263: 

this.bodyIsOverflowing = document.body.clientWidth < fullWindowWidth

=>

this.bodyIsOverflowing = this.$body.outerWidth(true) < fullWindowWidth